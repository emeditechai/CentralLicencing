using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Services
{
    public interface IWhatsAppEmailNotifier
    {
        /// <summary>
        /// Fire-and-forget: sends the email's content as a WhatsApp message to every recipient
        /// whose email belongs to an active user with a PhoneNumber. Never throws, never blocks the email.
        /// </summary>
        void QueueFromEmail(string? toEmails, string? subject, string? htmlBody,
            byte[]? attachmentBytes, string? attachmentFileName, string? emailType);
    }

    public class WhatsAppEmailNotifier : IWhatsAppEmailNotifier
    {
        public const string ModuleCode = "EMAIL";
        private const int MaxTextLength = 3500;

        // Email types that must not be mirrored (e.g. the SMTP test on the Email Engine page).
        private static readonly string[] SkippedEmailTypes = { "Email Engine Test" };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WhatsAppEmailNotifier> _logger;

        public WhatsAppEmailNotifier(IServiceScopeFactory scopeFactory, ILogger<WhatsAppEmailNotifier> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void QueueFromEmail(string? toEmails, string? subject, string? htmlBody,
            byte[]? attachmentBytes, string? attachmentFileName, string? emailType)
        {
            if (string.IsNullOrWhiteSpace(toEmails)) return;
            if (emailType != null && SkippedEmailTypes.Contains(emailType, StringComparer.OrdinalIgnoreCase)) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IWhatsAppRepository>();
                    var whatsApp = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

                    var cfg = await repo.GetConfigAsync();
                    if (cfg == null || !cfg.IsEnabled || !cfg.NotifyOnEmail || !cfg.HasApiKey) return;

                    var emails = toEmails.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(ExtractAddress)
                        .Where(e => e.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    if (emails.Count == 0) return;

                    var phones = (await repo.GetUserPhonesByEmailAsync(emails))
                        .Select(u => u.PhoneNumber.Trim())
                        .Distinct()
                        .ToList();
                    if (phones.Count == 0) return;

                    var text = BuildText(subject, htmlBody);

                    string? documentUrl = null;
                    if (attachmentBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(attachmentFileName))
                        documentUrl = await whatsApp.UploadMediaAsync(attachmentBytes, GuessMime(attachmentFileName));

                    foreach (var phone in phones)
                    {
                        await whatsApp.SendTextMessageAsync(phone, text, ModuleCode,
                            documentUrl: documentUrl,
                            fileName: documentUrl == null ? null : attachmentFileName,
                            sentBy: emailType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WhatsApp] Email mirror failed for {Recipients}", toEmails);
                }
            });
        }

        private static string ExtractAddress(string recipient)
        {
            var m = Regex.Match(recipient, "<([^>]+)>");
            return (m.Success ? m.Groups[1].Value : recipient).Trim();
        }

        /// <summary>Turns the email HTML into WhatsApp text: *subject* then the body with line breaks kept.</summary>
        public static string BuildText(string? subject, string? htmlBody)
        {
            var body = htmlBody ?? string.Empty;
            body = Regex.Replace(body, @"<(script|style|head)[^>]*>.*?</\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            body = Regex.Replace(body, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            body = Regex.Replace(body, @"</(p|div|tr|li|h[1-6]|table)>", "\n", RegexOptions.IgnoreCase);
            body = Regex.Replace(body, @"</t[dh]>", "  ", RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "<[^>]+>", "");
            body = WebUtility.HtmlDecode(body).Replace(' ', ' ');
            body = Regex.Replace(body, @"[ \t]+", " ");
            body = Regex.Replace(body, @" *\n *", "\n");
            body = Regex.Replace(body, @"\n{3,}", "\n\n").Trim();

            var text = string.IsNullOrWhiteSpace(subject) ? body : $"*{subject.Trim()}*\n\n{body}";
            return text.Length > MaxTextLength ? text[..MaxTextLength].TrimEnd() + "…" : text;
        }

        private static string GuessMime(string fileName) =>
            System.IO.Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
    }
}
