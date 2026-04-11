using System.Net;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Services
{
    public class TicketEmailService : ITicketEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ICompanySettingsRepository _companyRepo;
        private readonly string _connectionString;
        private readonly ILogger<TicketEmailService> _logger;

        public TicketEmailService(
            IEmailService emailService,
            ICompanySettingsRepository companyRepo,
            string connectionString,
            ILogger<TicketEmailService> logger)
        {
            _emailService = emailService;
            _companyRepo = companyRepo;
            _connectionString = connectionString;
            _logger = logger;
        }

        private SqlConnection CreateConnection() => new(_connectionString);

        // ── 1. Ticket Created → notify all Ticket Admin users ──
        public async Task NotifyTicketCreatedAsync(HelpDeskTicket ticket)
        {
            try
            {
                var admins = await GetTicketAdminEmailsAsync();
                if (!admins.Any()) return;

                var placeholders = new Dictionary<string, string>
                {
                    ["TicketNumber"] = ticket.TicketNumber,
                    ["Heading"] = "New Ticket Created",
                    ["IntroMessage"] = "A new support ticket has been created and requires attention.",
                    ["DetailsTable"] = BuildDetailsTableHtml(new Dictionary<string, string>
                    {
                        ["Ticket No"] = ticket.TicketNumber,
                        ["Subject"] = ticket.Subject,
                        ["Category"] = ticket.CategoryName ?? "—",
                        ["Priority"] = ticket.PriorityName ?? "Normal",
                        ["Created By"] = ticket.CreatedByName ?? "Unknown",
                        ["Status"] = ticket.Status
                    })
                };

                foreach (var admin in admins)
                {
                    await _emailService.SendTemplatedAsync("TICKET_CREATED", admin.Email, admin.FullName, placeholders);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ticket-created email for {TicketNumber}.", ticket.TicketNumber);
            }
        }

        // ── 2. Ticket Assigned → notify assignee + all Ticket Admin users ──
        public async Task NotifyTicketAssignedAsync(HelpDeskTicket ticket, string assigneeName, string assigneeEmail)
        {
            try
            {
                var detailsHtml = BuildDetailsTableHtml(new Dictionary<string, string>
                {
                    ["Ticket No"] = ticket.TicketNumber,
                    ["Subject"] = ticket.Subject,
                    ["Category"] = ticket.CategoryName ?? "—",
                    ["Priority"] = ticket.PriorityName ?? "Normal",
                    ["Created By"] = ticket.CreatedByName ?? "Unknown",
                    ["Assigned To"] = assigneeName,
                    ["Status"] = ticket.Status
                });

                // Send to the assigned user
                if (!string.IsNullOrWhiteSpace(assigneeEmail))
                {
                    await _emailService.SendTemplatedAsync("TICKET_ASSIGNED", assigneeEmail, assigneeName,
                        new Dictionary<string, string>
                        {
                            ["TicketNumber"] = ticket.TicketNumber,
                            ["Heading"] = "Ticket Assigned to You",
                            ["IntroMessage"] = "You have been assigned a support ticket. Please review and take action.",
                            ["DetailsTable"] = detailsHtml
                        });
                }

                // Also notify all Ticket Admins (except the assignee to avoid duplicate)
                var admins = await GetTicketAdminEmailsAsync();
                var adminPlaceholders = new Dictionary<string, string>
                {
                    ["TicketNumber"] = ticket.TicketNumber,
                    ["Heading"] = "Ticket Assignment Update",
                    ["IntroMessage"] = $"Ticket <strong>{Enc(ticket.TicketNumber)}</strong> has been assigned to <strong>{Enc(assigneeName)}</strong>.",
                    ["DetailsTable"] = detailsHtml
                };

                foreach (var admin in admins)
                {
                    if (!string.Equals(admin.Email, assigneeEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        await _emailService.SendTemplatedAsync("TICKET_ASSIGNED", admin.Email, admin.FullName, adminPlaceholders);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ticket-assigned email for {TicketNumber}.", ticket.TicketNumber);
            }
        }

        // ── 3. Status Changed → notify ticket creator + all Ticket Admin users ──
        public async Task NotifyStatusChangedAsync(HelpDeskTicket ticket, string oldStatus, string newStatus)
        {
            try
            {
                var detailsHtml = BuildDetailsTableHtml(new Dictionary<string, string>
                {
                    ["Ticket No"] = ticket.TicketNumber,
                    ["Subject"] = ticket.Subject,
                    ["Previous Status"] = oldStatus,
                    ["New Status"] = newStatus,
                    ["Created By"] = ticket.CreatedByName ?? "Unknown"
                });

                // Notify ticket creator
                var creator = await GetUserEmailAsync(ticket.CreatedById);
                if (creator != null)
                {
                    await _emailService.SendTemplatedAsync("TICKET_STATUS_CHANGED", creator.Email, creator.FullName,
                        new Dictionary<string, string>
                        {
                            ["TicketNumber"] = ticket.TicketNumber,
                            ["NewStatus"] = newStatus,
                            ["Heading"] = "Ticket Status Updated",
                            ["IntroMessage"] = $"The status of your ticket <strong>{Enc(ticket.TicketNumber)}</strong> has been changed from <strong>{Enc(oldStatus)}</strong> to <strong>{Enc(newStatus)}</strong>.",
                            ["DetailsTable"] = detailsHtml
                        });
                }

                // Notify Ticket Admins
                var admins = await GetTicketAdminEmailsAsync();
                var adminPlaceholders = new Dictionary<string, string>
                {
                    ["TicketNumber"] = ticket.TicketNumber,
                    ["NewStatus"] = newStatus,
                    ["Heading"] = "Ticket Status Changed",
                    ["IntroMessage"] = $"Ticket <strong>{Enc(ticket.TicketNumber)}</strong> status has been updated from <strong>{Enc(oldStatus)}</strong> to <strong>{Enc(newStatus)}</strong>.",
                    ["DetailsTable"] = detailsHtml
                };

                foreach (var admin in admins)
                {
                    if (creator != null && string.Equals(admin.Email, creator.Email, StringComparison.OrdinalIgnoreCase))
                        continue;

                    await _emailService.SendTemplatedAsync("TICKET_STATUS_CHANGED", admin.Email, admin.FullName, adminPlaceholders);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send status-changed email for {TicketNumber}.", ticket.TicketNumber);
            }
        }

        // ── 4. New Reply → notify creator + Ticket Admins. Internal notes skip creator. ──
        public async Task NotifyNewReplyAsync(HelpDeskTicket ticket, string replierName, string messageSnippet, bool isInternal)
        {
            try
            {
                var snippet = messageSnippet.Length > 200 ? messageSnippet[..200] + "…" : messageSnippet;
                var noteLabel = isInternal ? "Internal Note" : "New Reply";

                var detailsHtml = BuildDetailsTableHtml(new Dictionary<string, string>
                {
                    ["Ticket No"] = ticket.TicketNumber,
                    ["Subject"] = ticket.Subject,
                    ["Reply By"] = replierName,
                    ["Message Preview"] = snippet
                });

                // Notify ticket creator (only for public replies, NOT internal notes)
                string? creatorEmail = null;
                if (!isInternal)
                {
                    var creator = await GetUserEmailAsync(ticket.CreatedById);
                    if (creator != null)
                    {
                        creatorEmail = creator.Email;
                        await _emailService.SendTemplatedAsync("TICKET_REPLY", creator.Email, creator.FullName,
                            new Dictionary<string, string>
                            {
                                ["TicketNumber"] = ticket.TicketNumber,
                                ["NoteLabel"] = noteLabel,
                                ["Heading"] = $"{noteLabel} on Your Ticket",
                                ["IntroMessage"] = $"<strong>{Enc(replierName)}</strong> has replied to your ticket <strong>{Enc(ticket.TicketNumber)}</strong>.",
                                ["DetailsTable"] = detailsHtml
                            });
                    }
                }

                // Notify Ticket Admins (for both public and internal)
                var admins = await GetTicketAdminEmailsAsync();
                var adminPlaceholders = new Dictionary<string, string>
                {
                    ["TicketNumber"] = ticket.TicketNumber,
                    ["NoteLabel"] = noteLabel,
                    ["Heading"] = $"{noteLabel} Added",
                    ["IntroMessage"] = $"<strong>{Enc(replierName)}</strong> posted {(isInternal ? "an internal note" : "a reply")} on ticket <strong>{Enc(ticket.TicketNumber)}</strong>.",
                    ["DetailsTable"] = detailsHtml
                };

                foreach (var admin in admins)
                {
                    if (!string.IsNullOrEmpty(creatorEmail) &&
                        string.Equals(admin.Email, creatorEmail, StringComparison.OrdinalIgnoreCase))
                        continue;

                    await _emailService.SendTemplatedAsync("TICKET_REPLY", admin.Email, admin.FullName, adminPlaceholders);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reply email for {TicketNumber}.", ticket.TicketNumber);
            }
        }

        // ── Helpers ──

        private async Task<List<EmailRecipient>> GetTicketAdminEmailsAsync()
        {
            using var conn = CreateConnection();
            var results = await conn.QueryAsync<EmailRecipient>(@"
                SELECT DISTINCT u.Id, u.Email, ISNULL(u.FullName, u.Username) AS FullName
                FROM UserMaster u
                INNER JOIN UserRoleMap urm ON urm.UserId = u.Id
                INNER JOIN RoleMaster r ON r.Id = urm.RoleId
                WHERE u.IsActive = 1
                  AND r.RoleName = 'Ticket Admin'
                  AND ISNULL(u.Email, '') <> ''");
            return results.ToList();
        }

        private async Task<EmailRecipient?> GetUserEmailAsync(int userId)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmailRecipient>(@"
                SELECT Id, Email, ISNULL(FullName, Username) AS FullName
                FROM UserMaster
                WHERE Id = @UserId AND ISNULL(Email, '') <> ''",
                new { UserId = userId });
        }

        private static string Enc(string text) => WebUtility.HtmlEncode(text);

        /// <summary>Builds an HTML table with key–value detail rows for use as the {{DetailsTable}} placeholder.</summary>
        private static string BuildDetailsTableHtml(Dictionary<string, string> details)
        {
            var rows = string.Join("", details.Select(kv =>
                $@"<tr>
                    <td style=""padding:8px 12px;font-weight:600;color:#475569;white-space:nowrap;border-bottom:1px solid #f1f5f9;font-size:13px;"">{Enc(kv.Key)}</td>
                    <td style=""padding:8px 12px;color:#1e293b;border-bottom:1px solid #f1f5f9;font-size:13px;"">{Enc(kv.Value)}</td>
                </tr>"));

            return $@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;overflow:hidden;"">
                {rows}
            </table>";
        }

        private class EmailRecipient
        {
            public int Id { get; set; }
            public string Email { get; set; } = "";
            public string FullName { get; set; } = "";
        }
    }
}
