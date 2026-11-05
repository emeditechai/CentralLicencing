using System.Text.Json;
using CentralLicenceApp.Hubs;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WebPush;

namespace CentralLicenceApp.Services
{
    /// <summary>
    /// Sends Client Enquiry notifications. Runs on its own DI scope and never throws,
    /// so a failed reminder can't block saving a record.
    /// </summary>
    public interface IEnquiryNotificationService
    {
        void QueueUserMessage(int userId, string title, string text, string targetPath);
        void QueueDemoInvitation(int demoId, string company, string product, DateTime when, string? link, string kind);
        Task SendUserMessageAsync(int userId, string title, string text, string targetPath);
        Task SendClientDemoMessageAsync(int demoId, string reminderType, string company, string product, DateTime when, string? link, string kind);
    }

    public partial class EnquiryNotificationService : IEnquiryNotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<EnquiryNotificationService> _logger;

        public EnquiryNotificationService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<EnquiryNotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        public void QueueUserMessage(int userId, string title, string text, string targetPath)
            => _ = Task.Run(() => SendUserMessageAsync(userId, title, text, targetPath));

        public void QueueDemoInvitation(int demoId, string company, string product, DateTime when, string? link, string kind)
            => _ = Task.Run(() => SendClientDemoMessageAsync(demoId, $"INV_{when:yyyyMMddHHmm}_{StableHash(link ?? "") % 100000}", company, product, when, link, kind));

        private static int StableHash(string value)
        {
            var hash = 17;
            foreach (var ch in value.ToLowerInvariant()) hash = unchecked(hash * 31 + ch);
            return hash & int.MaxValue;
        }

        private string? AbsoluteUrl(string path)
        {
            var baseUrl = _config["AppUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl) || baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)) return null;
            return baseUrl + path;
        }

        public async Task SendUserMessageAsync(int userId, string title, string text, string targetPath)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sp = scope.ServiceProvider;
                var user = await sp.GetRequiredService<IEnquiryRepository>().GetNotifyUserAsync(userId);
                if (user == null) return;

                var payload = new BrowserTicketNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Category = "enquiry",
                    Title = title,
                    Message = text,
                    TargetUrl = targetPath,
                    RequireDocumentHidden = false
                };

                // In-app alert over the hub the layout is already connected to.
                try
                {
                    await sp.GetRequiredService<IHubContext<TicketNotificationHub>>().Clients.User(userId.ToString())
                        .SendAsync("TicketNotificationReceived", payload);
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[Enquiry] In-app alert failed for user {UserId}", userId); }

                await SendWebPushAsync(sp, userId, payload);

                var url = AbsoluteUrl(targetPath);
                var whatsApp = $"*{title}*\n\n{text}" + (url == null ? "" : $"\n\n🔗 {url}");
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    static string enc(string? v) => System.Net.WebUtility.HtmlEncode(v) ?? "";
                    var link = url == null ? "" : $"<p><a href=\"{enc(url)}\">Open in Central Licence</a></p>";
                    var html = $"<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1e293b;\"><h3 style=\"margin:0 0 10px;\">{enc(title)}</h3><p style=\"white-space:pre-line;\">{enc(text)}</p>{link}</div>";
                    // The email service also sends this text to the user's WhatsApp.
                    await sp.GetRequiredService<IEmailService>().SendAsync(user.Email, user.FullName ?? "", title, html, "Client Enquiry", whatsApp);
                }
                else if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    await sp.GetRequiredService<IWhatsAppService>().SendTextMessageAsync(user.PhoneNumber, whatsApp, "ENQUIRY");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Enquiry] Notification failed for user {UserId}", userId);
            }
        }

        private async Task SendWebPushAsync(IServiceProvider sp, int userId, BrowserTicketNotification payload)
        {
            try
            {
                var settings = sp.GetRequiredService<IOptions<PushNotificationSettings>>().Value;
                if (!settings.IsConfigured) return;
                var subscriptions = await sp.GetRequiredService<IUserPushSubscriptionRepository>().GetActiveByUserIdsAsync(new[] { userId });
                var client = new WebPushClient();
                var vapid = new VapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
                foreach (var s in subscriptions)
                {
                    try { await client.SendNotificationAsync(new PushSubscription(s.Endpoint, s.P256dh, s.Auth), JsonSerializer.Serialize(payload), vapid); }
                    catch (WebPushException ex) { _logger.LogDebug(ex, "[Enquiry] Web push rejected for user {UserId}", userId); }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "[Enquiry] Web push failed for user {UserId}", userId); }
        }

        public async Task SendClientDemoMessageAsync(int demoId, string reminderType, string company, string product, DateTime when, string? link, string kind)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sp = scope.ServiceProvider;
                var repo = sp.GetRequiredService<IEnquiryRepository>();
                var linkText = string.IsNullOrWhiteSpace(link) ? "" : $"\n\nMeeting link / address:\n{link}";
                static string enc(string? v) => System.Net.WebUtility.HtmlEncode(v) ?? "";
                var whatsApp = sp.GetRequiredService<IWhatsAppService>();
                var email = sp.GetRequiredService<IEmailService>();
                var subject = $"{product} demo on {when:dd MMM yyyy}";
                static string Html(string text) => $"<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;white-space:pre-line;\">{enc(text)}</div>";

                // People typed into Other attendees: each email or phone number found gets the message.
                foreach (var address in ParseOtherAttendees(await repo.GetDemoOtherAttendeesAsync(demoId)))
                {
                    if (!await repo.TryClaimReminderAsync("DemoOther", demoId, reminderType, StableHash(address))) continue;
                    var text = $"Hello, {kind} for the {product} demo with {company} on {when:dd MMM yyyy} at {when:hh:mm tt}.{linkText}";
                    if (address.Contains('@')) await email.SendAsync(address, address, subject, Html(text), "Enquiry Demo");
                    else await whatsApp.SendTextMessageAsync(address, text, "ENQ_DEMO", demoId);
                }

                foreach (var contact in await repo.GetDemoContactsAsync(demoId))
                {
                    // Contacts aren't users: a negative id keeps the claim separate from user reminders.
                    if (!await repo.TryClaimReminderAsync("Demo", demoId, reminderType, -contact.Id)) continue;
                    var text = $"Dear {contact.Name}, {kind} for your {product} demo with us on {when:dd MMM yyyy} at {when:hh:mm tt}.{linkText}";
                    var phone = string.IsNullOrWhiteSpace(contact.WhatsApp) ? contact.Mobile : contact.WhatsApp;
                    if (!contact.WhatsAppOptOut && !string.IsNullOrWhiteSpace(phone))
                        await whatsApp.SendTextMessageAsync(phone, text, "ENQ_DEMO", demoId);
                    if (!string.IsNullOrWhiteSpace(contact.Email))
                        await email.SendAsync(contact.Email, contact.Name, subject, Html(text), "Enquiry Demo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Enquiry] Client demo message failed for demo {DemoId}", demoId);
            }
        }
    }

    public partial class EnquiryNotificationService
    {
        private static IEnumerable<string> ParseOtherAttendees(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
            var found = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, @"[^\s,;<>()]+@[^\s,;<>()]+\.[^\s,;<>()]+|\+?\d[\d\- ]{8,}\d"))
            {
                var value = m.Value.Trim();
                if (!value.Contains('@')) value = (value.StartsWith('+') ? "+" : "") + new string(value.Where(char.IsDigit).ToArray());
                found.Add(value);
            }
            return found.Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>Every 5 minutes: follow-up and demo reminders, the morning digest, overdue escalation, and hold resumes.</summary>
    public class EnquiryReminderService : BackgroundService
    {
        private const int FollowUpLeadMinutes = 15;
        private const int DemoLeadMinutes = 60;
        private const int EscalateAfterHours = 24;
        private static readonly TimeSpan DigestFrom = TimeSpan.FromHours(9);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEnquiryNotificationService _notifier;
        private readonly ILogger<EnquiryReminderService> _logger;

        public EnquiryReminderService(IServiceScopeFactory scopeFactory, IEnquiryNotificationService notifier, ILogger<EnquiryReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _notifier = notifier;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try { await RunOnceAsync(); }
                catch (Exception ex) { _logger.LogError(ex, "[Enquiry] Reminder run failed"); }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task RunOnceAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEnquiryRepository>();

            await repo.ResumeHeldEnquiriesAsync();

            foreach (var f in await repo.GetFollowUpsDueWithinAsync(FollowUpLeadMinutes))
                if (await repo.TryClaimReminderAsync("FollowUp", f.RefId, "DUE", f.UserId))
                    await _notifier.SendUserMessageAsync(f.UserId, $"Follow-up due at {f.DueAt:hh:mm tt}",
                        $"{f.CompanyName} ({f.EnquiryNo})\n{f.ContactName} {f.Mobile}\n{f.Detail}", $"/ClientEnquiry/Details/{f.EnquiryId}");

            foreach (var f in await repo.GetOverdueForEscalationAsync(EscalateAfterHours))
                if (f.ManagerId.HasValue && await repo.TryClaimReminderAsync("FollowUp", f.RefId, "ESCALATE", f.ManagerId.Value))
                    await _notifier.SendUserMessageAsync(f.ManagerId.Value, "Follow-up overdue by over 24 hours",
                        $"{f.UserName} has an overdue follow-up.\n{f.CompanyName} ({f.EnquiryNo})\nWas due {f.DueAt:dd MMM yyyy hh:mm tt}", $"/ClientEnquiry/Details/{f.EnquiryId}");

            foreach (var d in await repo.GetDemosStartingWithinAsync(DemoLeadMinutes))
                if (await repo.TryClaimReminderAsync("Demo", d.RefId, "PRESENTER", d.UserId))
                {
                    await _notifier.SendUserMessageAsync(d.UserId, $"Demo at {d.DueAt:hh:mm tt}", $"{d.CompanyName} ({d.EnquiryNo})\n{d.Detail}", $"/EnquiryDemo/Details/{d.RefId}");
                    var product = d.Detail?.Split(" demo")[0] ?? "product";
                    var linkPart = d.Detail?.Contains(": ") == true ? d.Detail[(d.Detail.IndexOf(": ", StringComparison.Ordinal) + 2)..] : null;
                    await _notifier.SendClientDemoMessageAsync(d.RefId, "REMIND", d.CompanyName, product, d.DueAt, linkPart, "this is a reminder");
                }

            if (DateTime.Now.TimeOfDay >= DigestFrom)
            {
                var dayKey = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
                foreach (var row in await repo.GetDigestRowsAsync())
                    if (await repo.TryClaimReminderAsync("Digest", dayKey, "DAILY", row.UserId))
                        await _notifier.SendUserMessageAsync(row.UserId, "Your Call List for today",
                            $"{row.DueToday} follow-up(s) due today and {row.Overdue} overdue.", "/ClientEnquiry/CallList");
            }
        }
    }
}
