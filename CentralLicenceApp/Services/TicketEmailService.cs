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

                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = company?.CompanyName ?? "Help Desk";

                var subject = $"[{companyName}] New Ticket Created – {Enc(ticket.TicketNumber)}";
                var body = BuildEmailBody(companyName,
                    "New Ticket Created",
                    $"A new support ticket has been created and requires attention.",
                    new Dictionary<string, string>
                    {
                        ["Ticket No"] = ticket.TicketNumber,
                        ["Subject"] = ticket.Subject,
                        ["Category"] = ticket.CategoryName ?? "—",
                        ["Priority"] = ticket.PriorityName ?? "Normal",
                        ["Created By"] = ticket.CreatedByName ?? "Unknown",
                        ["Status"] = ticket.Status
                    });

                foreach (var admin in admins)
                {
                    await _emailService.SendAsync(admin.Email, admin.FullName, subject, body, "Ticket Created");
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
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = company?.CompanyName ?? "Help Desk";

                var subject = $"[{companyName}] Ticket Assigned to You – {Enc(ticket.TicketNumber)}";
                var body = BuildEmailBody(companyName,
                    "Ticket Assigned to You",
                    $"You have been assigned a support ticket. Please review and take action.",
                    new Dictionary<string, string>
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
                    await _emailService.SendAsync(assigneeEmail, assigneeName, subject, body, "Ticket Assigned");
                }

                // Also notify all Ticket Admins (except the assignee to avoid duplicate)
                var admins = await GetTicketAdminEmailsAsync();
                var adminSubject = $"[{companyName}] Ticket {Enc(ticket.TicketNumber)} Assigned to {Enc(assigneeName)}";
                var adminBody = BuildEmailBody(companyName,
                    "Ticket Assignment Update",
                    $"Ticket <strong>{Enc(ticket.TicketNumber)}</strong> has been assigned to <strong>{Enc(assigneeName)}</strong>.",
                    new Dictionary<string, string>
                    {
                        ["Ticket No"] = ticket.TicketNumber,
                        ["Subject"] = ticket.Subject,
                        ["Category"] = ticket.CategoryName ?? "—",
                        ["Priority"] = ticket.PriorityName ?? "Normal",
                        ["Created By"] = ticket.CreatedByName ?? "Unknown",
                        ["Assigned To"] = assigneeName,
                        ["Status"] = ticket.Status
                    });

                foreach (var admin in admins)
                {
                    if (!string.Equals(admin.Email, assigneeEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        await _emailService.SendAsync(admin.Email, admin.FullName, adminSubject, adminBody, "Ticket Assigned");
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
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = company?.CompanyName ?? "Help Desk";

                var details = new Dictionary<string, string>
                {
                    ["Ticket No"] = ticket.TicketNumber,
                    ["Subject"] = ticket.Subject,
                    ["Previous Status"] = oldStatus,
                    ["New Status"] = newStatus,
                    ["Created By"] = ticket.CreatedByName ?? "Unknown"
                };

                // Notify ticket creator
                var creator = await GetUserEmailAsync(ticket.CreatedById);
                if (creator != null)
                {
                    var subject = $"[{companyName}] Ticket {Enc(ticket.TicketNumber)} Status Updated – {Enc(newStatus)}";
                    var body = BuildEmailBody(companyName,
                        "Ticket Status Updated",
                        $"The status of your ticket <strong>{Enc(ticket.TicketNumber)}</strong> has been changed from <strong>{Enc(oldStatus)}</strong> to <strong>{Enc(newStatus)}</strong>.",
                        details);

                    await _emailService.SendAsync(creator.Email, creator.FullName, subject, body, "Ticket Status Changed");
                }

                // Notify Ticket Admins
                var admins = await GetTicketAdminEmailsAsync();
                var adminSubject = $"[{companyName}] Ticket {Enc(ticket.TicketNumber)} – Status: {Enc(newStatus)}";
                var adminBody = BuildEmailBody(companyName,
                    "Ticket Status Changed",
                    $"Ticket <strong>{Enc(ticket.TicketNumber)}</strong> status has been updated from <strong>{Enc(oldStatus)}</strong> to <strong>{Enc(newStatus)}</strong>.",
                    details);

                foreach (var admin in admins)
                {
                    // Skip if admin is the creator (already notified)
                    if (creator != null && string.Equals(admin.Email, creator.Email, StringComparison.OrdinalIgnoreCase))
                        continue;

                    await _emailService.SendAsync(admin.Email, admin.FullName, adminSubject, adminBody, "Ticket Status Changed");
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
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = company?.CompanyName ?? "Help Desk";

                var snippet = messageSnippet.Length > 200 ? messageSnippet[..200] + "…" : messageSnippet;
                var noteLabel = isInternal ? "Internal Note" : "New Reply";

                var details = new Dictionary<string, string>
                {
                    ["Ticket No"] = ticket.TicketNumber,
                    ["Subject"] = ticket.Subject,
                    ["Reply By"] = replierName,
                    ["Message Preview"] = snippet
                };

                // Notify ticket creator (only for public replies, NOT internal notes)
                string? creatorEmail = null;
                if (!isInternal)
                {
                    var creator = await GetUserEmailAsync(ticket.CreatedById);
                    if (creator != null)
                    {
                        creatorEmail = creator.Email;
                        var subject = $"[{companyName}] {noteLabel} on Ticket {Enc(ticket.TicketNumber)}";
                        var body = BuildEmailBody(companyName,
                            $"{noteLabel} on Your Ticket",
                            $"<strong>{Enc(replierName)}</strong> has replied to your ticket <strong>{Enc(ticket.TicketNumber)}</strong>.",
                            details);

                        await _emailService.SendAsync(creator.Email, creator.FullName, subject, body, "Ticket Reply");
                    }
                }

                // Notify Ticket Admins (for both public and internal)
                var admins = await GetTicketAdminEmailsAsync();
                var adminSubject = $"[{companyName}] {noteLabel} on Ticket {Enc(ticket.TicketNumber)}";
                var adminBody = BuildEmailBody(companyName,
                    $"{noteLabel} Added",
                    $"<strong>{Enc(replierName)}</strong> posted {(isInternal ? "an internal note" : "a reply")} on ticket <strong>{Enc(ticket.TicketNumber)}</strong>.",
                    details);

                foreach (var admin in admins)
                {
                    // Skip if admin is the creator and already notified
                    if (!string.IsNullOrEmpty(creatorEmail) &&
                        string.Equals(admin.Email, creatorEmail, StringComparison.OrdinalIgnoreCase))
                        continue;

                    await _emailService.SendAsync(admin.Email, admin.FullName, adminSubject, adminBody, "Ticket Reply");
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

        // ── Professional HTML Email Template ──
        private static string BuildEmailBody(string companyName, string heading, string introHtml, Dictionary<string, string> details)
        {
            var rows = string.Join("", details.Select(kv =>
                $@"<tr>
                    <td style=""padding:8px 12px;font-weight:600;color:#475569;white-space:nowrap;border-bottom:1px solid #f1f5f9;font-size:13px;"">{Enc(kv.Key)}</td>
                    <td style=""padding:8px 12px;color:#1e293b;border-bottom:1px solid #f1f5f9;font-size:13px;"">{Enc(kv.Value)}</td>
                </tr>"));

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""></head>
<body style=""margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Roboto,Arial,sans-serif;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f1f5f9;padding:32px 16px;"">
<tr><td align=""center"">
<table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.1);"">

    <!-- Header -->
    <tr>
        <td style=""background:linear-gradient(135deg,#4f46e5 0%,#7c3aed 100%);padding:28px 32px;text-align:center;"">
            <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-.3px;"">{Enc(companyName)}</h1>
            <p style=""margin:6px 0 0;color:rgba(255,255,255,.85);font-size:13px;font-weight:500;"">Help Desk Notification</p>
        </td>
    </tr>

    <!-- Body -->
    <tr>
        <td style=""padding:32px;"">
            <h2 style=""margin:0 0 12px;color:#1e293b;font-size:18px;font-weight:700;"">{Enc(heading)}</h2>
            <p style=""margin:0 0 24px;color:#475569;font-size:14px;line-height:1.6;"">{introHtml}</p>

            <!-- Details Table -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;overflow:hidden;"">
                {rows}
            </table>
        </td>
    </tr>

    <!-- Footer -->
    <tr>
        <td style=""padding:20px 32px;background:#f8fafc;border-top:1px solid #e2e8f0;text-align:center;"">
            <p style=""margin:0;color:#94a3b8;font-size:12px;"">This is an automated notification from {Enc(companyName)} Help Desk. Please do not reply to this email.</p>
        </td>
    </tr>

</table>
</td></tr>
</table>
</body>
</html>";
        }

        private class EmailRecipient
        {
            public int Id { get; set; }
            public string Email { get; set; } = "";
            public string FullName { get; set; } = "";
        }
    }
}
