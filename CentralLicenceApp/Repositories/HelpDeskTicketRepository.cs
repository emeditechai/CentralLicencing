using System.Data;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class HelpDeskTicketRepository : IHelpDeskTicketRepository
    {
        private readonly string _connectionString;

        public HelpDeskTicketRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        private const string TicketSelectSql = @"
            SELECT t.*,
                   c.CategoryName,
                   sc.SubCategoryName,
                   p.PriorityName,
                   p.ColorCode AS PriorityColor,
                   cr.FullName AS CreatedByName,
                   ag.FullName AS AssignedToName,
                   fy.FYCode
            FROM HelpDeskTicket t
            INNER JOIN TicketCategoryMaster c ON c.Id = t.CategoryId
            INNER JOIN TicketPriorityMaster p ON p.Id = t.PriorityId
            INNER JOIN UserMaster cr ON cr.Id = t.CreatedById
            LEFT  JOIN TicketSubCategoryMaster sc ON sc.Id = t.SubCategoryId
            LEFT  JOIN UserMaster ag ON ag.Id = t.AssignedToId
            LEFT  JOIN FinancialYearMaster fy ON fy.Id = t.FinancialYearId";

        // ── Ticket CRUD ──

        public async Task<IEnumerable<HelpDeskTicket>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<HelpDeskTicket>(
                $"{TicketSelectSql} ORDER BY t.CreatedAt DESC");
        }

        public async Task<IEnumerable<HelpDeskTicket>> GetByCreatorAsync(int userId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<HelpDeskTicket>(
                $"{TicketSelectSql} WHERE t.CreatedById = @UserId ORDER BY t.CreatedAt DESC",
                new { UserId = userId });
        }

        public async Task<IEnumerable<HelpDeskTicket>> GetByAssigneeAsync(int userId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<HelpDeskTicket>(
                $"{TicketSelectSql} WHERE t.AssignedToId = @UserId ORDER BY t.CreatedAt DESC",
                new { UserId = userId });
        }

        public async Task<HelpDeskTicket?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<HelpDeskTicket>(
                $"{TicketSelectSql} WHERE t.Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(HelpDeskTicket ticket)
        {
            using var conn = CreateConnection();
            ticket.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO HelpDeskTicket
                    (TicketNumber, Subject, Description, CategoryId, SubCategoryId, PriorityId, Status, CreatedById, AssignedToId, CreatedAt, FinancialYearId)
                VALUES
                    (@TicketNumber, @Subject, @Description, @CategoryId, @SubCategoryId, @PriorityId, @Status, @CreatedById, @AssignedToId, @CreatedAt, @FinancialYearId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, ticket);
        }

        public async Task<bool> UpdateStatusAsync(int ticketId, string status, DateTime? resolvedAt, DateTime? closedAt)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE HelpDeskTicket SET
                    Status     = @Status,
                    UpdatedAt  = GETDATE(),
                    ResolvedAt = COALESCE(@ResolvedAt, ResolvedAt),
                    ClosedAt   = COALESCE(@ClosedAt, ClosedAt)
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, new { Id = ticketId, Status = status, ResolvedAt = resolvedAt, ClosedAt = closedAt }) > 0;
        }

        public async Task<bool> AssignAsync(int ticketId, int agentId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE HelpDeskTicket SET AssignedToId = @AgentId, UpdatedAt = GETDATE() WHERE Id = @Id",
                new { Id = ticketId, AgentId = agentId }) > 0;
        }

        public async Task<bool> SetFirstResponseAsync(int ticketId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE HelpDeskTicket SET FirstResponseAt = GETDATE() WHERE Id = @Id AND FirstResponseAt IS NULL",
                new { Id = ticketId }) > 0;
        }

        public async Task<string> GenerateTicketNumberAsync()
        {
            using var conn = CreateConnection();
            // Financial year: Apr-Mar. If month >= 4 → FY starts this year, else last year
            var now = DateTime.Now;
            int fyStart = now.Month >= 4 ? now.Year : now.Year - 1;
            int fyEnd = fyStart + 1;
            var fyCode = $"{fyStart % 100:D2}{fyEnd % 100:D2}";
            var prefix = $"TKT-{fyCode}-";

            var maxNum = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 TicketNumber FROM HelpDeskTicket WHERE TicketNumber LIKE @Prefix + '%' ORDER BY Id DESC",
                new { Prefix = prefix });

            int next = 1;
            if (!string.IsNullOrEmpty(maxNum))
            {
                var parts = maxNum.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                    next = last + 1;
            }

            return $"{prefix}{next:D4}";
        }

        // ── Messages ──

        public async Task<IEnumerable<TicketMessage>> GetMessagesAsync(int ticketId)
        {
            using var conn = CreateConnection();
            var messages = (await conn.QueryAsync<TicketMessage>(@"
                SELECT m.*, u.FullName AS SenderName, u.ProfileImagePath AS SenderProfileImage
                FROM TicketMessage m
                INNER JOIN UserMaster u ON u.Id = m.SenderId
                WHERE m.TicketId = @TicketId
                ORDER BY m.CreatedAt ASC", new { TicketId = ticketId })).ToList();

            if (messages.Any())
            {
                var messageIds = messages.Select(m => m.Id).ToArray();
                var attachments = await conn.QueryAsync<TicketAttachment>(
                    "SELECT * FROM TicketAttachment WHERE MessageId IN @Ids", new { Ids = messageIds });

                foreach (var msg in messages)
                    msg.Attachments = attachments.Where(a => a.MessageId == msg.Id).ToList();
            }

            return messages;
        }

        public async Task<int> AddMessageAsync(TicketMessage message)
        {
            using var conn = CreateConnection();
            message.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketMessage (TicketId, SenderId, Message, IsInternal, CreatedAt)
                VALUES (@TicketId, @SenderId, @Message, @IsInternal, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, message);
        }

        // ── Attachments ──

        public async Task<IEnumerable<TicketAttachment>> GetAttachmentsAsync(int ticketId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketAttachment>(
                "SELECT * FROM TicketAttachment WHERE TicketId = @TicketId ORDER BY CreatedAt",
                new { TicketId = ticketId });
        }

        public async Task<TicketAttachment?> GetAttachmentByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TicketAttachment>(
                "SELECT * FROM TicketAttachment WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> AddAttachmentAsync(TicketAttachment attachment)
        {
            using var conn = CreateConnection();
            attachment.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketAttachment (TicketId, MessageId, FileName, FilePath, FileSize, UploadedById, CreatedAt)
                VALUES (@TicketId, @MessageId, @FileName, @FilePath, @FileSize, @UploadedById, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, attachment);
        }

        // ── Audit Log ──

        public async Task<IEnumerable<TicketAuditLog>> GetAuditLogsAsync(int ticketId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketAuditLog>(@"
                SELECT a.*, u.FullName AS PerformedByName
                FROM TicketAuditLog a
                INNER JOIN UserMaster u ON u.Id = a.PerformedById
                WHERE a.TicketId = @TicketId
                ORDER BY a.CreatedAt DESC", new { TicketId = ticketId });
        }

        public async Task<int> AddAuditLogAsync(TicketAuditLog log)
        {
            using var conn = CreateConnection();
            log.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketAuditLog (TicketId, Action, OldValue, NewValue, PerformedById, CreatedAt)
                VALUES (@TicketId, @Action, @OldValue, @NewValue, @PerformedById, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, log);
        }

        // ── Agents (users with Ticket Agent / Ticket Admin / Administrator roles) ──

        public async Task<IEnumerable<AgentOption>> GetAgentsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AgentOption>(@"
                SELECT DISTINCT u.Id, u.FullName
                FROM UserMaster u
                INNER JOIN UserRoleMap urm ON urm.UserId = u.Id
                INNER JOIN RoleMaster r ON r.Id = urm.RoleId
                WHERE u.IsActive = 1
                  AND r.RoleName IN ('Ticket Agent','Ticket Admin')
                ORDER BY u.FullName");
        }
    }
}
