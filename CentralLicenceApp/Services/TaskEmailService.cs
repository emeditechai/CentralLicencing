using System.Net;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Services
{
    /// <summary>
    /// Sends professional task-related email notifications using the configured SMTP engine.
    /// All emails are fire-and-forget (errors are logged, never rethrown) to avoid
    /// blocking the HTTP request pipeline.
    /// </summary>
    public class TaskEmailService : ITaskEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ICompanySettingsRepository _companyRepo;
        private readonly string _connectionString;
        private readonly ILogger<TaskEmailService> _logger;

        public TaskEmailService(
            IEmailService emailService,
            ICompanySettingsRepository companyRepo,
            string connectionString,
            ILogger<TaskEmailService> logger)
        {
            _emailService     = emailService;
            _companyRepo      = companyRepo;
            _connectionString = connectionString;
            _logger           = logger;
        }

        // ── 1. Task Created & Assigned ──────────────────────────────────────────
        public async Task NotifyTaskAssignedAsync(DailyTaskLog task, string assigneeName, string assigneeEmail)
        {
            if (string.IsNullOrWhiteSpace(assigneeEmail)) return;

            try
            {
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = ResolveCompanyName(company);

                var subject = task.TaskNumber != null ? $"[Task Assigned] {task.TaskNumber}: {task.TaskTitle}" : $"[Task Assigned] {task.TaskTitle}";
                var body = BuildTaskEmailHtml(
                    companyName:    companyName,
                    heading:        "A New Task Has Been Assigned to You",
                    intro:          $"Hi <strong>{Enc(assigneeName)}</strong>, a new task has been assigned to you. Please review the details below and take appropriate action.",
                    badgeLabel:     "NEW TASK",
                    badgeColor:     "#6366f1",
                    task:           task,
                    footerNote:     "You are receiving this email because you have been assigned to this task."
                );

                await _emailService.SendAsync(assigneeEmail, assigneeName, subject, body, "Task Assigned");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task-assigned email for TaskId={TaskId}.", task.Id);
            }
        }

        // ── 2. Task Updated ──────────────────────────────────────────────────────
        public async Task NotifyTaskUpdatedAsync(DailyTaskLog task, string assigneeName, string assigneeEmail,
            IEnumerable<string>? changedFields = null)
        {
            if (string.IsNullOrWhiteSpace(assigneeEmail)) return;

            try
            {
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = ResolveCompanyName(company);

                var subject = task.TaskNumber != null ? $"[Task Updated] {task.TaskNumber}: {task.TaskTitle}" : $"[Task Updated] {task.TaskTitle}";
                var body = BuildTaskEmailHtml(
                    companyName:    companyName,
                    heading:        "A Task Assigned to You Has Been Updated",
                    intro:          $"Hi <strong>{Enc(assigneeName)}</strong>, a task assigned to you has been updated. Please review the latest details and changes below.",
                    badgeLabel:     "UPDATED",
                    badgeColor:     "#0ea5e9",
                    task:           task,
                    footerNote:     "You are receiving this email because you are assigned to this task.",
                    changedFields:  changedFields
                );

                await _emailService.SendAsync(assigneeEmail, assigneeName, subject, body, "Task Updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task-updated email for TaskId={TaskId}.", task.Id);
            }
        }

        // ── 3. Task Comment Mention ──────────────────────────────────────────────
        public async Task NotifyTaskCommentMentionAsync(DailyTaskLog task, string senderName, string mentionedName, string mentionedEmail, string commentText)
        {
            if (string.IsNullOrWhiteSpace(mentionedEmail)) return;

            try
            {
                var company = await _companyRepo.GetParentCompanyAsync();
                var companyName = ResolveCompanyName(company);

                var subject = task.TaskNumber != null ? $"[Mention] You were mentioned in {task.TaskNumber}: {task.TaskTitle}" : $"[Mention] You were mentioned in a task: {task.TaskTitle}";
                var body = BuildTaskEmailHtml(
                    companyName:    companyName,
                    heading:        "You Were Mentioned in a Comment",
                    intro:          $"Hi <strong>{Enc(mentionedName)}</strong>, <strong>{Enc(senderName)}</strong> mentioned you in a comment on a task. Please review the comment below.",
                    badgeLabel:     "MENTION",
                    badgeColor:     "#8b5cf6",
                    task:           task,
                    footerNote:     "You are receiving this email because you were tagged in a comment.",
                    changedFields:  new List<string> { $"Comment: \"{commentText}\"" }
                );

                await _emailService.SendAsync(mentionedEmail, mentionedName, subject, body, "Task Mention");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task-mention email for TaskId={TaskId}.", task.Id);
            }
        }

        // ── HTML Email Builder ───────────────────────────────────────────────────

        private static string BuildTaskEmailHtml(
            string companyName,
            string heading,
            string intro,
            string badgeLabel,
            string badgeColor,
            DailyTaskLog task,
            string footerNote,
            IEnumerable<string>? changedFields = null)
        {
            // Resolve status colour
            var statusColor = task.Status switch
            {
                "Completed"  => "#16a34a",
                "In Progress" or "Started" => "#2563eb",
                "On Hold"    => "#dc2626",
                "Cancelled"  => "#6b7280",
                _            => "#d97706"   // Pending
            };

            // Build the details rows
            var rows = new (string Label, string Value)[]
            {
                ("Task Number",     task.TaskNumber ?? "—"),
                ("Task Title",      task.TaskTitle),
                ("Task Date",       task.TaskDate.ToString("dd MMM yyyy")),
                ("Task Type",       task.TaskTypeName),
                ("Category",        task.TaskCategoryName),
                ("Assigned To",     string.IsNullOrWhiteSpace(task.AssignedToUserName) ? "—" : task.AssignedToUserName),
                ("Created By",      task.UserName),
                ("Status",          task.Status),
                ("Project / Module",task.ProjectModuleName ?? "—"),
                ("Linked Ticket",   task.TicketNumber != null ? $"{task.TicketNumber} — {task.TicketSubject}" : "—"),
                ("Description",     string.IsNullOrWhiteSpace(task.Description) ? "—" : task.Description),
            };

            var detailsRowsHtml = string.Join("", rows.Select((r, i) =>
            {
                var bg = i % 2 == 0 ? "#ffffff" : "#f8fafc";

                // Status gets a coloured pill; Description gets a pre-wrap block; others plain text
                string valueHtml;
                if (r.Label == "Status")
                {
                    valueHtml = $"<span style=\"display:inline-block;padding:3px 12px;border-radius:20px;background:{statusColor}20;color:{statusColor};font-weight:700;font-size:12px;\">{Enc(r.Value)}</span>";
                }
                else if (r.Label == "Description")
                {
                    valueHtml = $"<span style=\"display:block;white-space:pre-wrap;color:#475569;font-weight:400;line-height:1.6;\">{Enc(r.Value)}</span>";
                }
                else
                {
                    valueHtml = $"<span style=\"color:#1e293b;font-weight:500;\">{Enc(r.Value)}</span>";
                }

                return $@"
                <tr>
                  <td style=""background:{bg};padding:10px 18px;font-size:13px;font-weight:700;color:#64748b;width:160px;border-bottom:1px solid #f1f5f9;white-space:nowrap;vertical-align:top;"">{Enc(r.Label)}</td>
                  <td style=""background:{bg};padding:10px 18px;font-size:13px;border-bottom:1px solid #f1f5f9;"">{valueHtml}</td>
                </tr>";
            }));

            // Build the "What Changed" block (only for updates that supply diff info)
            var changedList = changedFields?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var changesBlock = string.Empty;
            if (changedList != null && changedList.Count > 0)
            {
                var changeItems = string.Join("", changedList.Select(c =>
                    $"<li style=\"padding:5px 0;font-size:13px;color:#1e293b;border-bottom:1px solid #f1f5f9;\">{Enc(c)}</li>"));

                changesBlock = $@"
          <!-- What Changed -->
          <tr>
            <td style=""padding:0 36px 24px 36px;"">
              <div style=""font-size:11px;font-weight:800;text-transform:uppercase;letter-spacing:.06em;color:#0ea5e9;margin-bottom:10px;padding-bottom:8px;border-bottom:2px solid #e0f2fe;"">
                ✏️ What Changed
              </div>
              <ul style=""margin:0;padding:0 0 0 16px;list-style:disc;"">
                {changeItems}
              </ul>
            </td>
          </tr>";
            }

            var currentYear = DateTime.Now.Year;

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>{Enc(heading)}</title>
</head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;"">

  <!-- Outer wrapper -->
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f5f9;padding:32px 16px;"">
    <tr>
      <td align=""center"">
        <!-- Card -->
        <table role=""presentation"" width=""620"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);max-width:620px;width:100%;"">

          <!-- ── Header banner ── -->
          <tr>
            <td style=""background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);padding:32px 36px 28px 36px;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td>
                    <!-- Company logo area -->
                    <div style=""font-size:18px;font-weight:800;color:#ffffff;letter-spacing:.02em;margin-bottom:16px;"">
                      📋 {Enc(companyName)}
                    </div>
                    <!-- Badge -->
                    <span style=""display:inline-block;background:{badgeColor};color:#fff;font-size:11px;font-weight:800;letter-spacing:.08em;padding:4px 14px;border-radius:20px;text-transform:uppercase;margin-bottom:12px;"">{Enc(badgeLabel)}</span>
                    <!-- Heading -->
                    <h1 style=""margin:0;font-size:22px;font-weight:800;color:#ffffff;line-height:1.3;"">{Enc(heading)}</h1>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- ── Intro ── -->
          <tr>
            <td style=""padding:28px 36px 8px 36px;"">
              <p style=""margin:0;font-size:15px;color:#475569;line-height:1.7;"">{intro}</p>
            </td>
          </tr>

          <!-- ── Task details section ── -->
          <tr>
            <td style=""padding:20px 36px;"">
              <div style=""font-size:11px;font-weight:800;text-transform:uppercase;letter-spacing:.06em;color:#6366f1;margin-bottom:10px;padding-bottom:8px;border-bottom:2px solid #ede9fe;"">
                📌 Task Details
              </div>
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-radius:10px;overflow:hidden;border:1px solid #e2e8f0;"">
                {detailsRowsHtml}
              </table>
            </td>
          </tr>

          {changesBlock}

          <!-- ── Call-to-action ── -->
          <tr>
            <td style=""padding:4px 36px 32px 36px;text-align:center;"">
              <p style=""margin:0 0 18px 0;font-size:13px;color:#94a3b8;"">Log in to the portal to view the full task, log time, or update the status.</p>
            </td>
          </tr>

          <!-- ── Divider ── -->
          <tr>
            <td style=""border-top:1px solid #f1f5f9;padding:20px 36px;"">
              <p style=""margin:0;font-size:12px;color:#94a3b8;line-height:1.6;"">{Enc(footerNote)}</p>
              <p style=""margin:8px 0 0 0;font-size:11px;color:#cbd5e1;"">© {currentYear} {Enc(companyName)}. All rights reserved.</p>
            </td>
          </tr>

        </table>
        <!-- /Card -->
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static string ResolveCompanyName(CompanySetting? company) =>
            string.IsNullOrWhiteSpace(company?.CompanyName) ? "Emeditech Plus LLP" : company.CompanyName.Trim();

        private static string Enc(string? text) =>
            WebUtility.HtmlEncode(text ?? string.Empty);

        // Unused: connection string kept for future features (e.g., looking up managers)
        private SqlConnection CreateConnection() => new(_connectionString);
    }
}
