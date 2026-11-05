using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class EnquiryRepository : IEnquiryRepository
    {
        private static readonly HashSet<string> LookupTables = new(StringComparer.OrdinalIgnoreCase)
            { "EnquirySource", "EnquiryBusinessType", "EnquiryLostReason" };

        private readonly string _connectionString;

        public EnquiryRepository(string connectionString) { _connectionString = connectionString; }

        private SqlConnection CreateConnection() => new(_connectionString);

        public async Task EnsureTablesAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "086_CreateClientEnquiryTables.sql");
            if (!File.Exists(path)) return;
            using var conn = CreateConnection();
            var sql = (await File.ReadAllTextAsync(path)).Replace("\r\n", "\n");
            foreach (var batch in sql.Split("\nGO\n", StringSplitOptions.RemoveEmptyEntries))
                if (!string.IsNullOrWhiteSpace(batch)) await conn.ExecuteAsync(batch);
        }

        // ── Visibility ──────────────────────────────────────────────────────

        private const string ScopeSql = @"(@ScopeAll = 1 OR e.OwnerUserId IN @ScopeIds
            OR EXISTS (SELECT 1 FROM EnquiryCollaborator cb WHERE cb.EnquiryId = e.Id AND cb.UserId = @ScopeUser)
            OR EXISTS (SELECT 1 FROM EnquiryDemo dv WHERE dv.EnquiryId = e.Id AND dv.PresenterUserId = @ScopeUser))";

        private const string ClientScopeSql = @"(@ScopeAll = 1 OR c.CreatedByUserId IN @ScopeIds OR EXISTS (
            SELECT 1 FROM Enquiry e WHERE e.EnquiryClientId = c.Id AND e.IsDeleted = 0 AND " + ScopeSql + "))";

        private static DynamicParameters ScopeParams(EnquiryScope s)
        {
            var p = new DynamicParameters();
            p.Add("ScopeAll", s.All);
            p.Add("ScopeIds", s.UserIds);
            p.Add("ScopeUser", s.UserId);
            return p;
        }

        // ── Masters ─────────────────────────────────────────────────────────

        private static void CheckTable(string table)
        {
            if (!LookupTables.Contains(table)) throw new ArgumentException("Unknown lookup table", nameof(table));
        }

        public async Task<List<EnquiryLookup>> GetLookupsAsync(string table, bool activeOnly)
        {
            CheckTable(table);
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryLookup>(
                $"SELECT Id, Name, SortOrder, IsActive FROM {table} {(activeOnly ? "WHERE IsActive = 1" : "")} ORDER BY SortOrder, Name")).ToList();
        }

        public async Task<List<EnquiryStage>> GetStagesAsync(bool activeOnly)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryStage>(
                $"SELECT * FROM EnquiryStage {(activeOnly ? "WHERE IsActive = 1" : "")} ORDER BY SortOrder")).ToList();
        }

        public async Task<List<EnquiryCallOutcome>> GetOutcomesAsync(bool activeOnly)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCallOutcome>(
                $"SELECT * FROM EnquiryCallOutcome {(activeOnly ? "WHERE IsActive = 1" : "")} ORDER BY SortOrder")).ToList();
        }

        public async Task SaveLookupAsync(string table, EnquiryLookup item)
        {
            CheckTable(table);
            using var conn = CreateConnection();
            await conn.ExecuteAsync(item.Id == 0
                ? $"INSERT INTO {table} (Name, SortOrder, IsActive) VALUES (@Name, @SortOrder, @IsActive)"
                : $"UPDATE {table} SET Name = @Name, SortOrder = @SortOrder, IsActive = @IsActive WHERE Id = @Id",
                new { item.Id, item.Name, item.SortOrder, item.IsActive });
        }

        public async Task SaveStageAsync(EnquiryStage item)
        {
            using var conn = CreateConnection();
            // Stages with a fixed code drive automation, so their closed flag and active state stay fixed.
            await conn.ExecuteAsync(item.Id == 0
                ? @"INSERT INTO EnquiryStage (Name, SortOrder, IsActive, DefaultFollowUpDays, ColorCode) VALUES (@Name, @SortOrder, @IsActive, @DefaultFollowUpDays, @ColorCode)"
                : @"UPDATE EnquiryStage SET Name = @Name, SortOrder = @SortOrder, DefaultFollowUpDays = @DefaultFollowUpDays, ColorCode = @ColorCode,
                        IsActive = CASE WHEN StageCode IS NULL THEN @IsActive ELSE 1 END WHERE Id = @Id",
                new { item.Id, item.Name, item.SortOrder, item.IsActive, item.DefaultFollowUpDays, item.ColorCode });
        }

        public async Task SaveOutcomeAsync(EnquiryCallOutcome item)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(item.Id == 0
                ? @"INSERT INTO EnquiryCallOutcome (Name, SortOrder, IsActive, IsConnected, RetryHours) VALUES (@Name, @SortOrder, @IsActive, @IsConnected, @RetryHours)"
                : @"UPDATE EnquiryCallOutcome SET Name = @Name, SortOrder = @SortOrder, IsActive = @IsActive, IsConnected = @IsConnected, RetryHours = @RetryHours WHERE Id = @Id",
                new { item.Id, item.Name, item.SortOrder, item.IsActive, item.IsConnected, item.RetryHours });
        }

        // ── Enquiry Client Master ───────────────────────────────────────────

        private const string ClientSelect = @"
            SELECT c.*, bt.Name AS BusinessTypeName,
                   pc.Name AS PrimaryContactName, pc.Mobile AS PrimaryMobile, pc.Email AS PrimaryEmail,
                   (SELECT COUNT(*) FROM Enquiry x WHERE x.EnquiryClientId = c.Id AND x.IsDeleted = 0) AS EnquiryCount,
                   (SELECT COUNT(*) FROM Enquiry x INNER JOIN EnquiryStage s ON s.Id = x.StageId
                     WHERE x.EnquiryClientId = c.Id AND x.IsDeleted = 0 AND s.IsClosed = 0) AS OpenEnquiryCount
            FROM EnquiryClient c
            LEFT JOIN EnquiryBusinessType bt ON bt.Id = c.BusinessTypeId
            OUTER APPLY (SELECT TOP 1 * FROM EnquiryClientContact k WHERE k.EnquiryClientId = c.Id ORDER BY k.IsPrimary DESC, k.Id) pc";

        public async Task<List<EnquiryClient>> GetClientsAsync(EnquiryScope scope, string? search)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            var sql = ClientSelect + " WHERE " + ClientScopeSql;
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += @" AND (c.CompanyName LIKE @S OR c.ClientCode LIKE @S OR c.City LIKE @S OR c.GSTIN LIKE @S
                          OR EXISTS (SELECT 1 FROM EnquiryClientContact k2 WHERE k2.EnquiryClientId = c.Id
                                     AND (k2.Name LIKE @S OR k2.Mobile LIKE @S OR k2.Email LIKE @S)))";
                p.Add("S", $"%{search.Trim()}%");
            }
            return (await conn.QueryAsync<EnquiryClient>(sql + " ORDER BY c.CompanyName", p)).ToList();
        }

        public async Task<EnquiryClient?> GetClientAsync(int id, EnquiryScope scope)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            p.Add("Id", id);
            var client = await conn.QuerySingleOrDefaultAsync<EnquiryClient>(ClientSelect + " WHERE c.Id = @Id AND " + ClientScopeSql, p);
            if (client != null)
                client.Contacts = (await conn.QueryAsync<EnquiryClientContact>(
                    "SELECT * FROM EnquiryClientContact WHERE EnquiryClientId = @Id ORDER BY IsPrimary DESC, Id", new { Id = id })).ToList();
            return client;
        }

        public async Task<int> SaveClientAsync(EnquiryClient c)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            if (c.Id == 0)
            {
                c.Id = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO EnquiryClient (CompanyName, BusinessTypeId, GSTIN, Address, City, State, Website, ExistingSoftware, SizeInfo, ReferredBy, Notes, IsActive, CreatedByUserId)
                    VALUES (@CompanyName, @BusinessTypeId, @GSTIN, @Address, @City, @State, @Website, @ExistingSoftware, @SizeInfo, @ReferredBy, @Notes, @IsActive, @CreatedByUserId);
                    DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
                    UPDATE EnquiryClient SET ClientCode = 'EC-' + RIGHT('00000' + CAST(@NewId AS VARCHAR(10)), 5) WHERE Id = @NewId;
                    SELECT @NewId;", c, tx);
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE EnquiryClient SET CompanyName = @CompanyName, BusinessTypeId = @BusinessTypeId, GSTIN = @GSTIN, Address = @Address, City = @City, State = @State,
                        Website = @Website, ExistingSoftware = @ExistingSoftware, SizeInfo = @SizeInfo, ReferredBy = @ReferredBy, Notes = @Notes, IsActive = @IsActive, UpdatedAt = GETDATE()
                    WHERE Id = @Id", c, tx);
            }

            // Contacts: update by Id so call and demo history keep pointing at the same person.
            var keepIds = c.Contacts.Where(k => k.Id > 0).Select(k => k.Id).ToList();
            var existing = (await conn.QueryAsync<int>("SELECT Id FROM EnquiryClientContact WHERE EnquiryClientId = @Id", new { c.Id }, tx)).ToList();
            foreach (var removeId in existing.Except(keepIds))
            {
                var used = await conn.ExecuteScalarAsync<int>(@"SELECT (SELECT COUNT(*) FROM EnquiryCallLog WHERE ContactId = @R) + (SELECT COUNT(*) FROM EnquiryDemoAttendee WHERE ContactId = @R)", new { R = removeId }, tx);
                if (used == 0)
                    await conn.ExecuteAsync("DELETE FROM EnquiryClientContact WHERE Id = @R", new { R = removeId }, tx);
            }
            foreach (var k in c.Contacts)
            {
                k.EnquiryClientId = c.Id;
                if (k.Id > 0 && existing.Contains(k.Id))
                    await conn.ExecuteAsync(@"UPDATE EnquiryClientContact SET Name = @Name, Role = @Role, Mobile = @Mobile, WhatsApp = @WhatsApp, Email = @Email,
                        IsPrimary = @IsPrimary, WhatsAppOptOut = @WhatsAppOptOut WHERE Id = @Id", k, tx);
                else
                    await conn.ExecuteAsync(@"INSERT INTO EnquiryClientContact (EnquiryClientId, Name, Role, Mobile, WhatsApp, Email, IsPrimary, WhatsAppOptOut)
                        VALUES (@EnquiryClientId, @Name, @Role, @Mobile, @WhatsApp, @Email, @IsPrimary, @WhatsAppOptOut)", k, tx);
            }
            tx.Commit();
            return c.Id;
        }

        public async Task<List<EnquiryDuplicateMatch>> FindDuplicatesAsync(int excludeId, string? gstin, IEnumerable<string> mobiles, IEnumerable<string> emails)
        {
            var m = mobiles.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var e = emails.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryDuplicateMatch>(@"
                SELECT DISTINCT c.Id, c.CompanyName,
                    CASE WHEN @Gstin IS NOT NULL AND c.GSTIN = @Gstin THEN 'GSTIN'
                         WHEN k.Mobile IN @M THEN 'Mobile' ELSE 'Email' END AS MatchedOn
                FROM EnquiryClient c
                LEFT JOIN EnquiryClientContact k ON k.EnquiryClientId = c.Id
                WHERE c.Id <> @Ex AND ((@Gstin IS NOT NULL AND c.GSTIN = @Gstin) OR k.Mobile IN @M OR k.Email IN @E)",
                new { Ex = excludeId, Gstin = string.IsNullOrWhiteSpace(gstin) ? null : gstin.Trim(), M = m.Count == 0 ? new List<string> { "\u0001" } : m, E = e.Count == 0 ? new List<string> { "\u0001" } : e })).ToList();
        }

        public async Task<bool> SetClientActiveAsync(int id, bool active)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync("UPDATE EnquiryClient SET IsActive = @A, UpdatedAt = GETDATE() WHERE Id = @Id", new { Id = id, A = active }) > 0;
        }

        // ── Enquiries ───────────────────────────────────────────────────────

        private const string EnquirySelect = @"
            SELECT e.*, c.CompanyName, pc.Name AS PrimaryContactName, pc.Mobile AS PrimaryMobile,
                   so.Name AS SourceName, st.Name AS StageName, st.StageCode, st.ColorCode AS StageColor, st.IsClosed AS StageIsClosed,
                   pr.PriorityName, pr.ColorCode AS PriorityColor, u.FullName AS OwnerName, lr.Name AS LostReasonName, q.QuotationNo,
                   STUFF((SELECT ', ' + pm.ProductName FROM EnquiryProduct ep INNER JOIN ProductMaster pm ON pm.Id = ep.ProductId
                          WHERE ep.EnquiryId = e.Id ORDER BY pm.ProductName FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ProductNames
            FROM Enquiry e
            INNER JOIN EnquiryClient c ON c.Id = e.EnquiryClientId
            INNER JOIN EnquiryStage st ON st.Id = e.StageId
            LEFT JOIN EnquirySource so ON so.Id = e.SourceId
            LEFT JOIN TicketPriorityMaster pr ON pr.Id = e.PriorityId
            LEFT JOIN UserMaster u ON u.Id = e.OwnerUserId
            LEFT JOIN EnquiryLostReason lr ON lr.Id = e.LostReasonId
            LEFT JOIN Quotation q ON q.Id = e.QuotationId
            OUTER APPLY (SELECT TOP 1 * FROM EnquiryClientContact k WHERE k.EnquiryClientId = c.Id ORDER BY k.IsPrimary DESC, k.Id) pc";

        public async Task<List<Enquiry>> GetEnquiriesAsync(EnquiryScope scope, EnquiryFilter f)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            var sql = EnquirySelect + " WHERE e.IsDeleted = 0 AND " + ScopeSql;
            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                sql += @" AND (e.EnquiryNo LIKE @S OR e.Title LIKE @S OR c.CompanyName LIKE @S OR EXISTS
                    (SELECT 1 FROM EnquiryClientContact k3 WHERE k3.EnquiryClientId = c.Id AND (k3.Name LIKE @S OR k3.Mobile LIKE @S OR k3.Email LIKE @S)))";
                p.Add("S", $"%{f.Search.Trim()}%");
            }
            if (f.StageId.HasValue) { sql += " AND e.StageId = @StageId"; p.Add("StageId", f.StageId); }
            if (f.SourceId.HasValue) { sql += " AND e.SourceId = @SourceId"; p.Add("SourceId", f.SourceId); }
            if (f.PriorityId.HasValue) { sql += " AND e.PriorityId = @PriorityId"; p.Add("PriorityId", f.PriorityId); }
            if (f.OwnerId.HasValue) { sql += " AND e.OwnerUserId = @OwnerId"; p.Add("OwnerId", f.OwnerId); }
            if (f.ProductId.HasValue) { sql += " AND EXISTS (SELECT 1 FROM EnquiryProduct ep2 WHERE ep2.EnquiryId = e.Id AND ep2.ProductId = @ProductId)"; p.Add("ProductId", f.ProductId); }
            if (f.From.HasValue) { sql += " AND e.CreatedAt >= @From"; p.Add("From", f.From.Value.Date); }
            if (f.To.HasValue) { sql += " AND e.CreatedAt < @To"; p.Add("To", f.To.Value.Date.AddDays(1)); }
            sql += f.State switch
            {
                "open" => " AND st.IsClosed = 0",
                "closed" => " AND st.IsClosed = 1",
                "hold" => " AND e.IsOnHold = 1",
                "overdue" => " AND st.IsClosed = 0 AND e.IsOnHold = 0 AND e.NextFollowUpAt < GETDATE()",
                _ => ""
            };
            return (await conn.QueryAsync<Enquiry>(sql + " ORDER BY st.IsClosed, CASE WHEN e.NextFollowUpAt IS NULL THEN 1 ELSE 0 END, e.NextFollowUpAt, e.Id DESC", p)).ToList();
        }

        public async Task<Enquiry?> GetEnquiryAsync(int id, EnquiryScope scope)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            p.Add("Id", id);
            return await conn.QuerySingleOrDefaultAsync<Enquiry>(EnquirySelect + " WHERE e.Id = @Id AND e.IsDeleted = 0 AND " + ScopeSql, p);
        }

        public async Task<List<int>> GetEnquiryProductIdsAsync(int id)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<int>("SELECT ProductId FROM EnquiryProduct WHERE EnquiryId = @id", new { id })).ToList();
        }

        public async Task<List<int>> GetCollaboratorIdsAsync(int id)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<int>("SELECT UserId FROM EnquiryCollaborator WHERE EnquiryId = @id", new { id })).ToList();
        }

        private static Task ActivityAsync(IDbConnection conn, IDbTransaction tx, int enquiryId, string type, string summary, int? userId)
            => conn.ExecuteAsync("INSERT INTO EnquiryActivity (EnquiryId, ActivityType, Summary, UserId) VALUES (@enquiryId, @type, @summary, @userId)",
                new { enquiryId, type, summary = summary.Length > 1500 ? summary[..1500] : summary, userId }, tx);

        private static Task RecalcNextAsync(IDbConnection conn, IDbTransaction tx, int enquiryId)
            => conn.ExecuteAsync(@"UPDATE Enquiry SET UpdatedAt = GETDATE(),
                NextFollowUpAt = (SELECT MIN(DueAt) FROM EnquiryFollowUp WHERE EnquiryId = @enquiryId AND Status = 'Open') WHERE Id = @enquiryId", new { enquiryId }, tx);

        private static Task<int> InsertFollowUpAsync(IDbConnection conn, IDbTransaction tx, EnquiryFollowUp f, int userId)
            => conn.ExecuteScalarAsync<int>(@"
                INSERT INTO EnquiryFollowUp (EnquiryId, FollowUpType, DueAt, AssignedToUserId, Purpose, CreatedByUserId)
                VALUES (@EnquiryId, @FollowUpType, @DueAt, @AssignedToUserId, @Purpose, @userId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { f.EnquiryId, f.FollowUpType, f.DueAt, f.AssignedToUserId, f.Purpose, userId }, tx);

        // Moves forward only: automation never moves an enquiry backward or reopens a closed one.
        private static async Task AdvanceStageAsync(IDbConnection conn, IDbTransaction tx, int enquiryId, string stageCode, int userId)
        {
            var row = await conn.QuerySingleOrDefaultAsync<(int TargetId, string TargetName, int TargetSort, int CurrentSort, bool CurrentClosed)>(@"
                SELECT t.Id, t.Name, t.SortOrder, cur.SortOrder, cur.IsClosed
                FROM Enquiry e INNER JOIN EnquiryStage cur ON cur.Id = e.StageId
                CROSS JOIN EnquiryStage t WHERE e.Id = @enquiryId AND t.StageCode = @stageCode", new { enquiryId, stageCode }, tx);
            if (row.TargetId == 0 || row.CurrentClosed || row.TargetSort <= row.CurrentSort) return;
            await conn.ExecuteAsync("UPDATE Enquiry SET StageId = @Id, UpdatedAt = GETDATE() WHERE Id = @enquiryId", new { Id = row.TargetId, enquiryId }, tx);
            await ActivityAsync(conn, tx, enquiryId, "Stage", $"Stage moved to {row.TargetName} automatically.", userId);
        }

        private static async Task SaveChildrenAsync(IDbConnection conn, IDbTransaction tx, int id, IEnumerable<int> products, IEnumerable<int> collaborators)
        {
            await conn.ExecuteAsync("DELETE FROM EnquiryProduct WHERE EnquiryId = @id; DELETE FROM EnquiryCollaborator WHERE EnquiryId = @id;", new { id }, tx);
            foreach (var productId in products.Distinct())
                await conn.ExecuteAsync("INSERT INTO EnquiryProduct (EnquiryId, ProductId) VALUES (@id, @productId)", new { id, productId }, tx);
            foreach (var userId in collaborators.Distinct())
                await conn.ExecuteAsync("INSERT INTO EnquiryCollaborator (EnquiryId, UserId) VALUES (@id, @userId)", new { id, userId }, tx);
        }

        public async Task<int> CreateEnquiryAsync(Enquiry e, IEnumerable<int> productIds, IEnumerable<int> collaboratorIds, EnquiryFollowUp first, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var stageId = await conn.ExecuteScalarAsync<int>("SELECT Id FROM EnquiryStage WHERE StageCode = 'NEW'", transaction: tx);
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Enquiry (EnquiryClientId, Title, Requirement, SourceId, StageId, PriorityId, OwnerUserId, ExpectedValue, ExpectedCloseDate, CreatedByUserId)
                VALUES (@EnquiryClientId, @Title, @Requirement, @SourceId, @stageId, @PriorityId, @OwnerUserId, @ExpectedValue, @ExpectedCloseDate, @userId);
                DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
                UPDATE Enquiry SET EnquiryNo = 'ENQ-' + FORMAT(GETDATE(), 'yyMM') + '-' + RIGHT('00000' + CAST(@NewId AS VARCHAR(10)), 5) WHERE Id = @NewId;
                SELECT @NewId;",
                new { e.EnquiryClientId, e.Title, e.Requirement, e.SourceId, stageId, e.PriorityId, e.OwnerUserId, e.ExpectedValue, e.ExpectedCloseDate, userId }, tx);
            await SaveChildrenAsync(conn, tx, id, productIds, collaboratorIds);
            first.EnquiryId = id;
            first.AssignedToUserId = e.OwnerUserId;
            await InsertFollowUpAsync(conn, tx, first, userId);
            await RecalcNextAsync(conn, tx, id);
            await ActivityAsync(conn, tx, id, "Created", "Enquiry created.", userId);
            tx.Commit();
            return id;
        }

        public async Task UpdateEnquiryAsync(Enquiry e, IEnumerable<int> productIds, IEnumerable<int> collaboratorIds, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync(@"UPDATE Enquiry SET EnquiryClientId = @EnquiryClientId, Title = @Title, Requirement = @Requirement, SourceId = @SourceId, PriorityId = @PriorityId,
                ExpectedValue = @ExpectedValue, ExpectedCloseDate = @ExpectedCloseDate, UpdatedAt = GETDATE() WHERE Id = @Id",
                new { e.Id, e.EnquiryClientId, e.Title, e.Requirement, e.SourceId, e.PriorityId, e.ExpectedValue, e.ExpectedCloseDate }, tx);
            await SaveChildrenAsync(conn, tx, e.Id, productIds, collaboratorIds);
            await ActivityAsync(conn, tx, e.Id, "Edit", "Enquiry details updated.", userId);
            tx.Commit();
        }

        public async Task ChangeStageAsync(int id, EnquiryStage stage, int userId, DateTime? nextFollowUpAt, int? lostReasonId, string? lostRemarks, string? linkedClientCode)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var current = await conn.QuerySingleAsync<(string Name, int OwnerUserId)>(
                "SELECT s.Name, e.OwnerUserId FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.Id = @id", new { id }, tx);
            var isLost = stage.StageCode == EnquiryStageCodes.Lost;
            await conn.ExecuteAsync(@"UPDATE Enquiry SET StageId = @StageId, ClosedAt = @ClosedAt, LostReasonId = @LostReasonId, LostRemarks = @LostRemarks,
                IsOnHold = CASE WHEN @Closed = 1 THEN 0 ELSE IsOnHold END, UpdatedAt = GETDATE() WHERE Id = @id",
                new { id, StageId = stage.Id, ClosedAt = stage.IsClosed ? DateTime.Now : (DateTime?)null, LostReasonId = isLost ? lostReasonId : null,
                      LostRemarks = isLost ? lostRemarks : null, Closed = stage.IsClosed }, tx);
            if (stage.IsClosed)
                await conn.ExecuteAsync("UPDATE EnquiryFollowUp SET Status = 'Cancelled' WHERE EnquiryId = @id AND Status = 'Open'", new { id }, tx);
            if (nextFollowUpAt.HasValue)
                await InsertFollowUpAsync(conn, tx, new EnquiryFollowUp { EnquiryId = id, DueAt = nextFollowUpAt.Value, AssignedToUserId = current.OwnerUserId, Purpose = $"Follow up after moving to {stage.Name}" }, userId);
            if (stage.StageCode == EnquiryStageCodes.Won && !string.IsNullOrWhiteSpace(linkedClientCode))
                await conn.ExecuteAsync(@"UPDATE EnquiryClient SET LinkedClientCode = @linkedClientCode WHERE Id = (SELECT EnquiryClientId FROM Enquiry WHERE Id = @id)", new { id, linkedClientCode }, tx);
            await RecalcNextAsync(conn, tx, id);
            var text = $"Stage changed from {current.Name} to {stage.Name}.";
            if (isLost && !string.IsNullOrWhiteSpace(lostRemarks)) text += $" Remarks: {lostRemarks}";
            await ActivityAsync(conn, tx, id, "Stage", text, userId);
            tx.Commit();
        }

        public async Task ReassignAsync(int id, int newOwnerId, string newOwnerName, string reason, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("UPDATE Enquiry SET OwnerUserId = @newOwnerId, UpdatedAt = GETDATE() WHERE Id = @id", new { id, newOwnerId }, tx);
            await conn.ExecuteAsync("UPDATE EnquiryFollowUp SET AssignedToUserId = @newOwnerId WHERE EnquiryId = @id AND Status = 'Open'", new { id, newOwnerId }, tx);
            await ActivityAsync(conn, tx, id, "Reassign", $"Reassigned to {newOwnerName}. Reason: {reason}", userId);
            tx.Commit();
        }

        public async Task SetHoldAsync(int id, DateTime? resumeDate, string? reason, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("UPDATE Enquiry SET IsOnHold = @Hold, ResumeDate = @resumeDate, UpdatedAt = GETDATE() WHERE Id = @id",
                new { id, resumeDate, Hold = resumeDate.HasValue }, tx);
            await ActivityAsync(conn, tx, id, "Stage",
                resumeDate.HasValue ? $"Put on hold until {resumeDate:dd MMM yyyy}. {reason}".Trim() : "Resumed from hold.", userId);
            tx.Commit();
        }

        public async Task SoftDeleteAsync(int id, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("UPDATE Enquiry SET IsDeleted = 1, UpdatedAt = GETDATE() WHERE Id = @id", new { id }, tx);
            await conn.ExecuteAsync("UPDATE EnquiryFollowUp SET Status = 'Cancelled' WHERE EnquiryId = @id AND Status = 'Open'", new { id }, tx);
            await ActivityAsync(conn, tx, id, "Edit", "Enquiry deleted.", userId);
            tx.Commit();
        }

        public async Task AddNoteAsync(int id, string note, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await ActivityAsync(conn, tx, id, "Note", note, userId);
            tx.Commit();
        }

        public async Task LinkQuotationAsync(int id, int quotationId, string quotationNo, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("UPDATE Enquiry SET QuotationId = @quotationId, UpdatedAt = GETDATE() WHERE Id = @id", new { id, quotationId }, tx);
            await ActivityAsync(conn, tx, id, "Quotation", $"Quotation {quotationNo} linked.", userId);
            await AdvanceStageAsync(conn, tx, id, EnquiryStageCodes.QuotationSent, userId);
            tx.Commit();
        }

        // ── Follow-ups and calls ────────────────────────────────────────────

        public async Task<List<EnquiryFollowUp>> GetFollowUpsAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryFollowUp>(@"SELECT f.*, u.FullName AS AssignedToName FROM EnquiryFollowUp f
                LEFT JOIN UserMaster u ON u.Id = f.AssignedToUserId WHERE f.EnquiryId = @enquiryId
                ORDER BY CASE WHEN f.Status = 'Open' THEN 0 ELSE 1 END, f.DueAt DESC", new { enquiryId })).ToList();
        }

        public async Task<EnquiryFollowUp?> GetFollowUpAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EnquiryFollowUp>("SELECT * FROM EnquiryFollowUp WHERE Id = @id", new { id });
        }

        public async Task<int> AddFollowUpAsync(EnquiryFollowUp f, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var id = await InsertFollowUpAsync(conn, tx, f, userId);
            await RecalcNextAsync(conn, tx, f.EnquiryId);
            await ActivityAsync(conn, tx, f.EnquiryId, "FollowUp", $"{f.FollowUpType} follow-up planned for {f.DueAt:dd MMM yyyy hh:mm tt}. {f.Purpose}".Trim(), userId);
            tx.Commit();
            return id;
        }

        public async Task CompleteFollowUpAsync(int id, string result, EnquiryFollowUp? next, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var f = await conn.QuerySingleAsync<EnquiryFollowUp>("SELECT * FROM EnquiryFollowUp WHERE Id = @id", new { id }, tx);
            await conn.ExecuteAsync("UPDATE EnquiryFollowUp SET Status = 'Done', Result = @result, CompletedAt = GETDATE() WHERE Id = @id", new { id, result }, tx);
            await ActivityAsync(conn, tx, f.EnquiryId, "FollowUp", $"{f.FollowUpType} follow-up completed: {result}", userId);
            if (next != null)
            {
                next.EnquiryId = f.EnquiryId;
                await InsertFollowUpAsync(conn, tx, next, userId);
                await ActivityAsync(conn, tx, f.EnquiryId, "FollowUp", $"Next {next.FollowUpType} follow-up planned for {next.DueAt:dd MMM yyyy hh:mm tt}.", userId);
            }
            await AdvanceStageAsync(conn, tx, f.EnquiryId, EnquiryStageCodes.Contacted, userId);
            await RecalcNextAsync(conn, tx, f.EnquiryId);
            tx.Commit();
        }

        public async Task RescheduleFollowUpAsync(int id, DateTime newDue, string reason, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var f = await conn.QuerySingleAsync<EnquiryFollowUp>("SELECT * FROM EnquiryFollowUp WHERE Id = @id", new { id }, tx);
            await conn.ExecuteAsync(@"UPDATE EnquiryFollowUp SET DueAt = @newDue, RescheduleCount = RescheduleCount + 1, LastRescheduleReason = @reason WHERE Id = @id", new { id, newDue, reason }, tx);
            // A reminder for the old time must not stop the reminder for the new time.
            await conn.ExecuteAsync("DELETE FROM EnquiryReminderLog WHERE RefType = 'FollowUp' AND RefId = @id", new { id }, tx);
            await RecalcNextAsync(conn, tx, f.EnquiryId);
            await ActivityAsync(conn, tx, f.EnquiryId, "FollowUp", $"Follow-up rescheduled from {f.DueAt:dd MMM yyyy hh:mm tt} to {newDue:dd MMM yyyy hh:mm tt}. Reason: {reason}", userId);
            tx.Commit();
        }

        public async Task<int> OpenFollowUpCountAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EnquiryFollowUp WHERE EnquiryId = @enquiryId AND Status = 'Open'", new { enquiryId });
        }

        public async Task LogCallAsync(EnquiryCallLog call, bool connected, EnquiryFollowUp? next, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            call.CalledByUserId = userId;
            await conn.ExecuteAsync(@"INSERT INTO EnquiryCallLog (EnquiryId, ContactId, FollowUpId, Direction, PhoneNumber, StartedAt, DurationSeconds, OutcomeId, Notes, CalledByUserId)
                VALUES (@EnquiryId, @ContactId, @FollowUpId, @Direction, @PhoneNumber, @StartedAt, @DurationSeconds, @OutcomeId, @Notes, @CalledByUserId)", call, tx);
            var outcome = await conn.ExecuteScalarAsync<string>("SELECT Name FROM EnquiryCallOutcome WHERE Id = @OutcomeId", new { call.OutcomeId }, tx);
            await ActivityAsync(conn, tx, call.EnquiryId, "Call", $"{call.Direction} call to {call.PhoneNumber ?? "client"}: {outcome}. {call.Notes}".Trim(), userId);
            if (call.FollowUpId.HasValue)
                await conn.ExecuteAsync(@"UPDATE EnquiryFollowUp SET Status = 'Done', Result = @Result, CompletedAt = GETDATE() WHERE Id = @Id AND EnquiryId = @EnquiryId AND Status = 'Open'",
                    new { Id = call.FollowUpId, call.EnquiryId, Result = $"{outcome}. {call.Notes}".Trim() }, tx);
            if (next != null)
            {
                next.EnquiryId = call.EnquiryId;
                await InsertFollowUpAsync(conn, tx, next, userId);
            }
            if (connected) await AdvanceStageAsync(conn, tx, call.EnquiryId, EnquiryStageCodes.Contacted, userId);
            await RecalcNextAsync(conn, tx, call.EnquiryId);
            tx.Commit();
        }

        public async Task<List<EnquiryCallLog>> GetCallsAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCallLog>(@"SELECT l.*, o.Name AS OutcomeName, u.FullName AS CalledByName, k.Name AS ContactName
                FROM EnquiryCallLog l INNER JOIN EnquiryCallOutcome o ON o.Id = l.OutcomeId
                LEFT JOIN UserMaster u ON u.Id = l.CalledByUserId LEFT JOIN EnquiryClientContact k ON k.Id = l.ContactId
                WHERE l.EnquiryId = @enquiryId ORDER BY l.StartedAt DESC", new { enquiryId })).ToList();
        }

        public async Task<List<EnquiryCallListItem>> GetCallListAsync(IEnumerable<int> userIds)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCallListItem>(@"
                SELECT f.Id AS FollowUpId, f.EnquiryId, e.EnquiryNo, f.FollowUpType, f.DueAt, f.Purpose, f.RescheduleCount,
                       c.CompanyName, pc.Name AS ContactName, pc.Id AS ContactId, pc.Mobile, pc.WhatsApp,
                       st.Name AS StageName, st.StageCode, st.ColorCode AS StageColor, pr.PriorityName, pr.ColorCode AS PriorityColor,
                       u.FullName AS AssignedToName,
                       STUFF((SELECT ', ' + pm.ProductName FROM EnquiryProduct ep INNER JOIN ProductMaster pm ON pm.Id = ep.ProductId
                              WHERE ep.EnquiryId = e.Id ORDER BY pm.ProductName FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ProductNames,
                       (SELECT COUNT(*) FROM EnquiryCallLog a WHERE a.EnquiryId = e.Id) AS Attempts,
                       lc.Summary AS LastCallSummary, lc.StartedAt AS LastCallAt
                FROM EnquiryFollowUp f
                INNER JOIN Enquiry e ON e.Id = f.EnquiryId AND e.IsDeleted = 0
                INNER JOIN EnquiryClient c ON c.Id = e.EnquiryClientId
                INNER JOIN EnquiryStage st ON st.Id = e.StageId AND st.IsClosed = 0
                LEFT JOIN TicketPriorityMaster pr ON pr.Id = e.PriorityId
                LEFT JOIN UserMaster u ON u.Id = f.AssignedToUserId
                OUTER APPLY (SELECT TOP 1 * FROM EnquiryClientContact k WHERE k.EnquiryClientId = c.Id ORDER BY k.IsPrimary DESC, k.Id) pc
                OUTER APPLY (SELECT TOP 1 l.StartedAt, o.Name + ISNULL(': ' + l.Notes, '') AS Summary FROM EnquiryCallLog l
                             INNER JOIN EnquiryCallOutcome o ON o.Id = l.OutcomeId WHERE l.EnquiryId = e.Id ORDER BY l.StartedAt DESC) lc
                WHERE f.Status = 'Open' AND f.AssignedToUserId IN @userIds
                  AND (e.IsOnHold = 0 OR e.ResumeDate <= CAST(GETDATE() AS DATE))
                ORDER BY CASE WHEN f.DueAt < GETDATE() THEN 0 ELSE 1 END, ISNULL(pr.SortOrder, 0) DESC, f.DueAt", new { userIds })).ToList();
        }

        // ── Demos ───────────────────────────────────────────────────────────

        private const string DemoSelect = @"
            SELECT d.*, pm.ProductName, u.FullName AS PresenterName, e.EnquiryNo, c.CompanyName, e.EnquiryClientId
            FROM EnquiryDemo d
            INNER JOIN Enquiry e ON e.Id = d.EnquiryId AND e.IsDeleted = 0
            INNER JOIN EnquiryClient c ON c.Id = e.EnquiryClientId
            INNER JOIN ProductMaster pm ON pm.Id = d.ProductId
            LEFT JOIN UserMaster u ON u.Id = d.PresenterUserId";

        public async Task<List<EnquiryDemo>> GetDemosAsync(EnquiryScope scope, DateTime from, DateTime to)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            p.Add("From", from);
            p.Add("To", to);
            return (await conn.QueryAsync<EnquiryDemo>(DemoSelect + " WHERE d.ScheduledAt >= @From AND d.ScheduledAt < @To AND " + ScopeSql + " ORDER BY d.ScheduledAt", p)).ToList();
        }

        public async Task<List<EnquiryDemo>> GetEnquiryDemosAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryDemo>(DemoSelect + " WHERE d.EnquiryId = @enquiryId ORDER BY d.ScheduledAt DESC", new { enquiryId })).ToList();
        }

        public async Task<EnquiryDemo?> GetDemoAsync(int id, EnquiryScope scope)
        {
            using var conn = CreateConnection();
            var p = ScopeParams(scope);
            p.Add("Id", id);
            var d = await conn.QuerySingleOrDefaultAsync<EnquiryDemo>(DemoSelect + " WHERE d.Id = @Id AND " + ScopeSql, p);
            if (d != null)
                d.AttendeeContactIds = (await conn.QueryAsync<int>("SELECT ContactId FROM EnquiryDemoAttendee WHERE DemoId = @Id", new { Id = id })).ToList();
            return d;
        }

        public async Task<int> SaveDemoAsync(EnquiryDemo d, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            if (d.Id == 0)
            {
                d.Id = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO EnquiryDemo (EnquiryId, ProductId, Mode, ScheduledAt, DurationMinutes, PresenterUserId, MeetingLink, Attendees, CreatedByUserId)
                    VALUES (@EnquiryId, @ProductId, @Mode, @ScheduledAt, @DurationMinutes, @PresenterUserId, @MeetingLink, @Attendees, @userId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { d.EnquiryId, d.ProductId, d.Mode, d.ScheduledAt, d.DurationMinutes, d.PresenterUserId, d.MeetingLink, d.Attendees, userId }, tx);
                await ActivityAsync(conn, tx, d.EnquiryId, "Demo", $"Demo scheduled for {d.ScheduledAt:dd MMM yyyy hh:mm tt}.", userId);
                await AdvanceStageAsync(conn, tx, d.EnquiryId, EnquiryStageCodes.DemoScheduled, userId);
            }
            else
            {
                var old = await conn.QuerySingleAsync<DateTime>("SELECT ScheduledAt FROM EnquiryDemo WHERE Id = @Id", new { d.Id }, tx);
                var moved = old != d.ScheduledAt;
                await conn.ExecuteAsync(@"UPDATE EnquiryDemo SET ProductId = @ProductId, Mode = @Mode, ScheduledAt = @ScheduledAt, DurationMinutes = @DurationMinutes,
                    PresenterUserId = @PresenterUserId, MeetingLink = @MeetingLink, Attendees = @Attendees,
                    Status = CASE WHEN @moved = 1 THEN 'Rescheduled' ELSE Status END WHERE Id = @Id",
                    new { d.Id, d.ProductId, d.Mode, d.ScheduledAt, d.DurationMinutes, d.PresenterUserId, d.MeetingLink, d.Attendees, moved }, tx);
                if (moved)
                {
                    await conn.ExecuteAsync("DELETE FROM EnquiryReminderLog WHERE RefType = 'Demo' AND RefId = @Id", new { d.Id }, tx);
                    await ActivityAsync(conn, tx, d.EnquiryId, "Demo", $"Demo rescheduled from {old:dd MMM yyyy hh:mm tt} to {d.ScheduledAt:dd MMM yyyy hh:mm tt}.", userId);
                }
            }
            await conn.ExecuteAsync("DELETE FROM EnquiryDemoAttendee WHERE DemoId = @Id", new { d.Id }, tx);
            foreach (var contactId in d.AttendeeContactIds.Distinct())
                await conn.ExecuteAsync("INSERT INTO EnquiryDemoAttendee (DemoId, ContactId) VALUES (@Id, @contactId)", new { d.Id, contactId }, tx);
            tx.Commit();
            return d.Id;
        }

        public async Task<int> PresenterClashCountAsync(int presenterId, DateTime start, int minutes, int excludeDemoId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM EnquiryDemo WHERE PresenterUserId = @presenterId AND Id <> @excludeDemoId AND Status IN ('Scheduled', 'Rescheduled')
                  AND ScheduledAt < @End AND DATEADD(MINUTE, DurationMinutes, ScheduledAt) > @start",
                new { presenterId, excludeDemoId, start, End = start.AddMinutes(minutes) });
        }

        public async Task CompleteDemoAsync(int id, int? rating, string? interest, string? feedback, EnquiryFollowUp next, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var enquiryId = await conn.ExecuteScalarAsync<int>("SELECT EnquiryId FROM EnquiryDemo WHERE Id = @id", new { id }, tx);
            await conn.ExecuteAsync("UPDATE EnquiryDemo SET Status = 'Completed', Rating = @rating, InterestLevel = @interest, Feedback = @feedback WHERE Id = @id", new { id, rating, interest, feedback }, tx);
            await ActivityAsync(conn, tx, enquiryId, "Demo", $"Demo completed. Interest: {interest ?? "—"}. {feedback}".Trim(), userId);
            next.EnquiryId = enquiryId;
            await InsertFollowUpAsync(conn, tx, next, userId);
            await AdvanceStageAsync(conn, tx, enquiryId, EnquiryStageCodes.DemoDone, userId);
            await RecalcNextAsync(conn, tx, enquiryId);
            tx.Commit();
        }

        public async Task CloseDemoAsync(int id, string status, string? reason, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var enquiryId = await conn.ExecuteScalarAsync<int>("SELECT EnquiryId FROM EnquiryDemo WHERE Id = @id", new { id }, tx);
            await conn.ExecuteAsync("UPDATE EnquiryDemo SET Status = @status, Feedback = @reason WHERE Id = @id", new { id, status, reason }, tx);
            await ActivityAsync(conn, tx, enquiryId, "Demo", $"Demo marked {status}. {reason}".Trim(), userId);
            tx.Commit();
        }

        // ── Timeline, attachments ───────────────────────────────────────────

        public async Task<List<EnquiryActivity>> GetActivitiesAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryActivity>(@"SELECT a.Id, a.ActivityType, a.Summary, a.CreatedAt, u.FullName AS UserName
                FROM EnquiryActivity a LEFT JOIN UserMaster u ON u.Id = a.UserId WHERE a.EnquiryId = @enquiryId ORDER BY a.CreatedAt DESC, a.Id DESC", new { enquiryId })).ToList();
        }

        public async Task<List<EnquiryAttachment>> GetAttachmentsAsync(int enquiryId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryAttachment>(@"SELECT a.*, u.FullName AS UploadedByName FROM EnquiryAttachment a
                LEFT JOIN UserMaster u ON u.Id = a.UploadedByUserId WHERE a.EnquiryId = @enquiryId ORDER BY a.Id DESC", new { enquiryId })).ToList();
        }

        public async Task<EnquiryAttachment?> GetAttachmentAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EnquiryAttachment>("SELECT * FROM EnquiryAttachment WHERE Id = @id", new { id });
        }

        public async Task AddAttachmentAsync(EnquiryAttachment a, int userId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("INSERT INTO EnquiryAttachment (EnquiryId, FileName, StoredName, UploadedByUserId) VALUES (@EnquiryId, @FileName, @StoredName, @userId)",
                new { a.EnquiryId, a.FileName, a.StoredName, userId }, tx);
            await ActivityAsync(conn, tx, a.EnquiryId, "Attachment", $"Attached {a.FileName}.", userId);
            tx.Commit();
        }

        // ── Dashboard and reports ───────────────────────────────────────────

        public async Task<EnquiryDashboardSummary> GetSummaryAsync(EnquiryScope scope)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleAsync<EnquiryDashboardSummary>(@"
                SELECT
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.IsClosed = 0 AND " + ScopeSql + @") AS OpenEnquiries,
                    (SELECT ISNULL(SUM(e.ExpectedValue), 0) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.IsClosed = 0 AND " + ScopeSql + @") AS OpenValue,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.IsClosed = 0 AND e.IsOnHold = 0
                        AND e.NextFollowUpAt >= GETDATE() AND e.NextFollowUpAt < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) AND " + ScopeSql + @") AS DueToday,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.IsClosed = 0 AND e.IsOnHold = 0 AND e.NextFollowUpAt < GETDATE() AND " + ScopeSql + @") AS Overdue,
                    (SELECT COUNT(*) FROM EnquiryDemo d INNER JOIN Enquiry e ON e.Id = d.EnquiryId WHERE e.IsDeleted = 0 AND d.Status IN ('Scheduled', 'Rescheduled') AND d.ScheduledAt >= GETDATE() AND " + ScopeSql + @") AS UpcomingDemos,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.StageCode = 'WON' AND " + ScopeSql + @") AS Won,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.IsDeleted = 0 AND s.StageCode = 'LOST' AND " + ScopeSql + @") AS Lost",
                ScopeParams(scope));
        }

        public async Task<List<EnquiryCountRow>> GetPipelineAsync(EnquiryScope scope)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCountRow>(@"
                SELECT s.Name, s.ColorCode AS Color, COUNT(e.Id) AS Total, ISNULL(SUM(e.ExpectedValue), 0) AS Value
                FROM EnquiryStage s LEFT JOIN Enquiry e ON e.StageId = s.Id AND e.IsDeleted = 0 AND " + ScopeSql + @"
                WHERE s.IsActive = 1 GROUP BY s.Name, s.ColorCode, s.SortOrder ORDER BY s.SortOrder", ScopeParams(scope))).ToList();
        }

        private static DynamicParameters RangeParams(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            var p = ScopeParams(scope);
            p.Add("From", from?.Date);
            p.Add("To", to?.Date.AddDays(1));
            return p;
        }

        public async Task<List<EnquiryCountRow>> GetBySourceAsync(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCountRow>(@"
                SELECT ISNULL(so.Name, 'Not set') AS Name, COUNT(*) AS Total, SUM(CASE WHEN st.StageCode = 'WON' THEN 1 ELSE 0 END) AS Won, ISNULL(SUM(e.ExpectedValue), 0) AS Value
                FROM Enquiry e INNER JOIN EnquiryStage st ON st.Id = e.StageId LEFT JOIN EnquirySource so ON so.Id = e.SourceId
                WHERE e.IsDeleted = 0 AND (@From IS NULL OR e.CreatedAt >= @From) AND (@To IS NULL OR e.CreatedAt < @To) AND " + ScopeSql + @"
                GROUP BY so.Name ORDER BY Total DESC", RangeParams(scope, from, to))).ToList();
        }

        public async Task<List<EnquiryCountRow>> GetLostReasonsAsync(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCountRow>(@"
                SELECT ISNULL(lr.Name, 'Not set') AS Name, COUNT(*) AS Total, ISNULL(SUM(e.ExpectedValue), 0) AS Value
                FROM Enquiry e INNER JOIN EnquiryStage st ON st.Id = e.StageId AND st.StageCode = 'LOST' LEFT JOIN EnquiryLostReason lr ON lr.Id = e.LostReasonId
                WHERE e.IsDeleted = 0 AND (@From IS NULL OR e.ClosedAt >= @From) AND (@To IS NULL OR e.ClosedAt < @To) AND " + ScopeSql + @"
                GROUP BY lr.Name ORDER BY Total DESC", RangeParams(scope, from, to))).ToList();
        }

        public async Task<List<EnquiryCountRow>> GetCallOutcomesAsync(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCountRow>(@"
                SELECT o.Name, COUNT(*) AS Total
                FROM EnquiryCallLog l INNER JOIN EnquiryCallOutcome o ON o.Id = l.OutcomeId INNER JOIN Enquiry e ON e.Id = l.EnquiryId
                WHERE e.IsDeleted = 0 AND (@From IS NULL OR l.StartedAt >= @From) AND (@To IS NULL OR l.StartedAt < @To) AND " + ScopeSql + @"
                GROUP BY o.Name ORDER BY Total DESC", RangeParams(scope, from, to))).ToList();
        }

        public async Task<List<EnquiryCountRow>> GetDemoOutcomesAsync(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryCountRow>(@"
                SELECT d.Status AS Name, COUNT(*) AS Total
                FROM EnquiryDemo d INNER JOIN Enquiry e ON e.Id = d.EnquiryId
                WHERE e.IsDeleted = 0 AND (@From IS NULL OR d.ScheduledAt >= @From) AND (@To IS NULL OR d.ScheduledAt < @To) AND " + ScopeSql + @"
                GROUP BY d.Status ORDER BY Total DESC", RangeParams(scope, from, to))).ToList();
        }

        public async Task<List<EnquiryUserActivityRow>> GetUserActivityAsync(EnquiryScope scope, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryUserActivityRow>(@"
                SELECT u.FullName AS UserName,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.OwnerUserId = u.Id AND e.IsDeleted = 0 AND s.IsClosed = 0) AS OpenEnquiries,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.OwnerUserId = u.Id AND e.IsDeleted = 0 AND s.StageCode = 'WON' AND (@From IS NULL OR e.ClosedAt >= @From) AND (@To IS NULL OR e.ClosedAt < @To)) AS Won,
                    (SELECT COUNT(*) FROM Enquiry e INNER JOIN EnquiryStage s ON s.Id = e.StageId WHERE e.OwnerUserId = u.Id AND e.IsDeleted = 0 AND s.StageCode = 'LOST' AND (@From IS NULL OR e.ClosedAt >= @From) AND (@To IS NULL OR e.ClosedAt < @To)) AS Lost,
                    (SELECT COUNT(*) FROM EnquiryCallLog l WHERE l.CalledByUserId = u.Id AND (@From IS NULL OR l.StartedAt >= @From) AND (@To IS NULL OR l.StartedAt < @To)) AS Calls,
                    (SELECT COUNT(*) FROM EnquiryCallLog l INNER JOIN EnquiryCallOutcome o ON o.Id = l.OutcomeId WHERE o.IsConnected = 1 AND l.CalledByUserId = u.Id AND (@From IS NULL OR l.StartedAt >= @From) AND (@To IS NULL OR l.StartedAt < @To)) AS ConnectedCalls,
                    (SELECT COUNT(*) FROM EnquiryFollowUp f WHERE f.AssignedToUserId = u.Id AND f.Status = 'Done' AND (@From IS NULL OR f.CompletedAt >= @From) AND (@To IS NULL OR f.CompletedAt < @To)) AS FollowUpsDone,
                    (SELECT COUNT(*) FROM EnquiryFollowUp f INNER JOIN Enquiry e ON e.Id = f.EnquiryId WHERE f.AssignedToUserId = u.Id AND f.Status = 'Open' AND e.IsDeleted = 0 AND e.IsOnHold = 0 AND f.DueAt < GETDATE()) AS Overdue,
                    (SELECT COUNT(*) FROM EnquiryDemo d WHERE d.PresenterUserId = u.Id AND d.Status = 'Completed' AND (@From IS NULL OR d.ScheduledAt >= @From) AND (@To IS NULL OR d.ScheduledAt < @To)) AS Demos
                FROM UserMaster u
                WHERE u.IsActive = 1 AND (@ScopeAll = 1 OR u.Id IN @ScopeIds)
                  AND (EXISTS (SELECT 1 FROM Enquiry e WHERE e.OwnerUserId = u.Id AND e.IsDeleted = 0) OR EXISTS (SELECT 1 FROM EnquiryCallLog l WHERE l.CalledByUserId = u.Id))
                ORDER BY u.FullName", RangeParams(scope, from, to))).ToList();
        }

        public async Task<List<EnquiryQuotationOption>> GetQuotationOptionsAsync()
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryQuotationOption>("SELECT TOP 300 Id, QuotationNo, PartyName, TotalAmount FROM Quotation ORDER BY Id DESC")).ToList();
        }

        // ── Reminders ───────────────────────────────────────────────────────

        public async Task<bool> TryClaimReminderAsync(string refType, int refId, string reminderType, int userId)
        {
            using var conn = CreateConnection();
            try
            {
                await conn.ExecuteAsync("INSERT INTO EnquiryReminderLog (RefType, RefId, ReminderType, UserId) VALUES (@refType, @refId, @reminderType, @userId)",
                    new { refType, refId, reminderType, userId });
                return true;
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                return false; // already sent
            }
        }

        private const string ReminderFollowUpSelect = @"
            SELECT f.Id AS RefId, f.EnquiryId, e.EnquiryNo, c.CompanyName, pc.Name AS ContactName, pc.Mobile, f.DueAt,
                   f.FollowUpType + ISNULL(': ' + f.Purpose, '') AS Detail, f.AssignedToUserId AS UserId, u.ManagerId, u.FullName AS UserName
            FROM EnquiryFollowUp f
            INNER JOIN Enquiry e ON e.Id = f.EnquiryId AND e.IsDeleted = 0 AND e.IsOnHold = 0
            INNER JOIN EnquiryStage st ON st.Id = e.StageId AND st.IsClosed = 0
            INNER JOIN EnquiryClient c ON c.Id = e.EnquiryClientId
            INNER JOIN UserMaster u ON u.Id = f.AssignedToUserId
            OUTER APPLY (SELECT TOP 1 * FROM EnquiryClientContact k WHERE k.EnquiryClientId = c.Id ORDER BY k.IsPrimary DESC, k.Id) pc
            WHERE f.Status = 'Open' ";

        public async Task<List<EnquiryReminderItem>> GetFollowUpsDueWithinAsync(int minutes)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryReminderItem>(ReminderFollowUpSelect + "AND f.DueAt >= GETDATE() AND f.DueAt <= DATEADD(MINUTE, @minutes, GETDATE())", new { minutes })).ToList();
        }

        public async Task<List<EnquiryReminderItem>> GetOverdueForEscalationAsync(int hours)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryReminderItem>(ReminderFollowUpSelect + "AND f.DueAt < DATEADD(HOUR, -@hours, GETDATE()) AND u.ManagerId IS NOT NULL", new { hours })).ToList();
        }

        public async Task<List<EnquiryDigestRow>> GetDigestRowsAsync()
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryDigestRow>(@"
                SELECT f.AssignedToUserId AS UserId,
                    SUM(CASE WHEN f.DueAt >= CAST(GETDATE() AS DATE) AND f.DueAt < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) AS DueToday,
                    SUM(CASE WHEN f.DueAt < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS Overdue
                FROM EnquiryFollowUp f
                INNER JOIN Enquiry e ON e.Id = f.EnquiryId AND e.IsDeleted = 0 AND e.IsOnHold = 0
                INNER JOIN EnquiryStage st ON st.Id = e.StageId AND st.IsClosed = 0
                WHERE f.Status = 'Open' AND f.DueAt < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
                GROUP BY f.AssignedToUserId")).ToList();
        }

        public async Task<List<EnquiryReminderItem>> GetDemosStartingWithinAsync(int minutes)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryReminderItem>(@"
                SELECT d.Id AS RefId, d.EnquiryId, e.EnquiryNo, c.CompanyName, d.ScheduledAt AS DueAt,
                       pm.ProductName + ' demo' + ISNULL(': ' + d.MeetingLink, '') AS Detail, d.PresenterUserId AS UserId, u.FullName AS UserName
                FROM EnquiryDemo d
                INNER JOIN Enquiry e ON e.Id = d.EnquiryId AND e.IsDeleted = 0
                INNER JOIN EnquiryClient c ON c.Id = e.EnquiryClientId
                INNER JOIN ProductMaster pm ON pm.Id = d.ProductId
                INNER JOIN UserMaster u ON u.Id = d.PresenterUserId
                WHERE d.Status IN ('Scheduled', 'Rescheduled') AND d.ScheduledAt >= GETDATE() AND d.ScheduledAt <= DATEADD(MINUTE, @minutes, GETDATE())", new { minutes })).ToList();
        }

        public async Task<EnquiryNotifyUser?> GetNotifyUserAsync(int userId)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EnquiryNotifyUser>("SELECT Id, FullName, Email, PhoneNumber FROM UserMaster WHERE Id = @userId AND IsActive = 1", new { userId });
        }

        public async Task<List<EnquiryClientContact>> GetDemoContactsAsync(int demoId)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<EnquiryClientContact>(@"SELECT k.* FROM EnquiryDemoAttendee a INNER JOIN EnquiryClientContact k ON k.Id = a.ContactId WHERE a.DemoId = @demoId", new { demoId })).ToList();
        }

        public async Task<string?> GetDemoOtherAttendeesAsync(int demoId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<string?>("SELECT Attendees FROM EnquiryDemo WHERE Id = @demoId", new { demoId });
        }

        public async Task<int> ResumeHeldEnquiriesAsync()
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                INSERT INTO EnquiryActivity (EnquiryId, ActivityType, Summary)
                SELECT Id, 'Stage', 'Resumed from hold automatically.' FROM Enquiry WHERE IsOnHold = 1 AND IsDeleted = 0 AND ResumeDate <= CAST(GETDATE() AS DATE);
                UPDATE Enquiry SET IsOnHold = 0, UpdatedAt = GETDATE() WHERE IsOnHold = 1 AND IsDeleted = 0 AND ResumeDate <= CAST(GETDATE() AS DATE);");
        }
    }
}
