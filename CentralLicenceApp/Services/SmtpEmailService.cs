using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CentralLicenceApp.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IMailConfigRepository _mailConfigRepo;
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly IEmailLogRepository _emailLogRepo;
        private readonly ICompanySettingsRepository _companySettingsRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IMailConfigRepository mailConfigRepo,
            IEmailTemplateRepository templateRepo,
            IEmailLogRepository emailLogRepo,
            ICompanySettingsRepository companySettingsRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SmtpEmailService> logger)
        {
            _mailConfigRepo = mailConfigRepo;
            _templateRepo   = templateRepo;
            _emailLogRepo = emailLogRepo;
            _companySettingsRepo = companySettingsRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger         = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, string? emailType = null)
        {
            await SendCoreAsync(string.IsNullOrWhiteSpace(emailType) ? "Direct Email" : emailType, null, toEmail, toName, subject, htmlBody, null, null);
        }

        public async Task SendWithAttachmentAsync(string toEmail, string toName, string subject, string htmlBody,
            byte[] attachmentBytes, string attachmentFileName, string? emailType = null)
        {
            await SendCoreAsync(string.IsNullOrWhiteSpace(emailType) ? "Direct Email" : emailType, null, toEmail, toName, subject, htmlBody, attachmentBytes, attachmentFileName);
        }

        private async Task SendCoreAsync(string emailType, string? templateKey, string? toEmail, string? toName,
            string? subject, string? htmlBody, byte[]? attachmentBytes, string? attachmentFileName)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                await LogEmailAsync(emailType, templateKey, toEmail, toName, subject, htmlBody, "Skipped", "Recipient email address was empty.");
                return;
            }

            var config = await _mailConfigRepo.GetActiveAsync();
            if (config == null)
            {
                _logger.LogWarning("No active mail configuration found. Email not sent to {Email}.", toEmail);
                await LogEmailAsync(emailType, templateKey, toEmail, toName, subject, htmlBody, "Skipped", "No active mail configuration found.");
                return;
            }

            try
            {
                var message = new MimeMessage();

                // Sender & Reply-To (using dynamic config)
                var senderName = string.IsNullOrWhiteSpace(config.FromName) ? config.FromEmail : config.FromName;
                var senderAddress = new MailboxAddress(senderName, config.FromEmail);
                message.From.Add(senderAddress);
                message.ReplyTo.Add(senderAddress);

                // Recipient(s) - handle single or comma/semicolon delimited email addresses
                var recipientList = toEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var recipient in recipientList)
                {
                    if (MailboxAddress.TryParse(recipient, out var parsedAddress))
                    {
                        if (string.IsNullOrWhiteSpace(parsedAddress.Name) && !string.IsNullOrWhiteSpace(toName) && recipientList.Length == 1)
                        {
                            message.To.Add(new MailboxAddress(toName, parsedAddress.Address));
                        }
                        else
                        {
                            message.To.Add(parsedAddress);
                        }
                    }
                    else
                    {
                        message.To.Add(new MailboxAddress(toName ?? string.Empty, recipient));
                    }
                }

                message.Subject = subject ?? string.Empty;

                // Message Body (HTML + Plain text fallback)
                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody ?? string.Empty,
                    TextBody = !string.IsNullOrWhiteSpace(htmlBody)
                        ? Regex.Replace(htmlBody, "<[^>]+>", " ").Trim()
                        : string.Empty
                };

                // Attachment (if any)
                if (attachmentBytes != null && attachmentBytes.Length > 0 && !string.IsNullOrWhiteSpace(attachmentFileName))
                {
                    builder.Attachments.Add(attachmentFileName, attachmentBytes);
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 25000; // 25s timeout

                // Accept self-signed / corporate TLS certificates to prevent unexpected handshake disconnects
                client.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;

                // Set EHLO/HELO local domain based on sender or username domain for corporate mail filter compliance
                var localDomain = ResolveLocalDomain(config);
                if (!string.IsNullOrWhiteSpace(localDomain))
                {
                    client.LocalDomain = localDomain;
                }

                // Resolve SecureSocketOptions dynamically based on Port and EnableSSL
                var secureSocketOptions = ResolveSecureSocketOptions(config.SmtpPort, config.EnableSSL);

                await client.ConnectAsync(config.SmtpServer, config.SmtpPort, secureSocketOptions);

                // Authenticate if credentials are provided
                if (!string.IsNullOrWhiteSpace(config.SmtpUsername))
                {
                    await client.AuthenticateAsync(config.SmtpUsername, config.SmtpPassword ?? string.Empty);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email successfully sent to {Email} via {SmtpServer}:{Port} | Subject: {Subject}",
                    toEmail, config.SmtpServer, config.SmtpPort, subject);

                await LogEmailAsync(emailType, templateKey, toEmail, toName, subject, htmlBody, "Sent", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} via {SmtpServer}:{Port}.",
                    toEmail, config?.SmtpServer, config?.SmtpPort);

                await LogEmailAsync(emailType, templateKey, toEmail, toName, subject, htmlBody, "Failed", ex.Message);
            }
        }

        public async Task SendTemplatedAsync(string templateKey, string toEmail, string toName,
            Dictionary<string, string> placeholders)
        {
            var template = await _templateRepo.GetByKeyAsync(templateKey);
            if (template == null)
            {
                _logger.LogWarning("Email template '{Key}' not found or inactive.", templateKey);
                await LogEmailAsync(templateKey, templateKey, toEmail, toName, null, null, "Skipped", $"Email template '{templateKey}' not found or inactive.");
                return;
            }

            var company = await _companySettingsRepo.GetParentCompanyAsync();
            var companyName = ResolveCompanyName(company);
            var templateValues = placeholders == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(placeholders, StringComparer.OrdinalIgnoreCase);

            templateValues["CompanyName"] = companyName;
            templateValues["AppName"] = companyName;

            var subject = ReplaceBranding(ReplacePlaceholders(template.Subject, templateValues), companyName);
            var body = ReplaceBranding(ReplacePlaceholders(template.Body, templateValues), companyName);

            await SendCoreAsync(template.TemplateName, template.TemplateKey, toEmail, toName, subject, body, null, null);
        }

        public async Task<(string Subject, string Body)?> ResolveTemplateAsync(string templateKey, Dictionary<string, string> placeholders)
        {
            var template = await _templateRepo.GetByKeyAsync(templateKey);
            if (template == null)
            {
                _logger.LogWarning("Email template '{Key}' not found or inactive.", templateKey);
                return null;
            }

            var company = await _companySettingsRepo.GetParentCompanyAsync();
            var companyName = ResolveCompanyName(company);
            var templateValues = placeholders == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(placeholders, StringComparer.OrdinalIgnoreCase);

            templateValues["CompanyName"] = companyName;
            templateValues["AppName"] = companyName;

            var subject = ReplaceBranding(ReplacePlaceholders(template.Subject, templateValues), companyName);
            var body = ReplaceBranding(ReplacePlaceholders(template.Body, templateValues), companyName);

            return (subject, body);
        }

        private async Task LogEmailAsync(string emailType, string? templateKey, string? toEmail, string? toName,
            string? subject, string? htmlBody, string status, string? errorMessage)
        {
            try
            {
                await _emailLogRepo.CreateAsync(new EmailLogEntry
                {
                    EmailType = string.IsNullOrWhiteSpace(emailType) ? "Direct Email" : emailType,
                    TemplateKey = templateKey,
                    RecipientEmail = toEmail,
                    RecipientName = toName,
                    Subject = subject,
                    Body = htmlBody,
                    Status = status,
                    ErrorMessage = errorMessage,
                    TriggeredBy = ResolveTriggeredBy(),
                    CreatedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist email log for {Email}.", toEmail);
            }
        }

        private string ResolveTriggeredBy()
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return string.IsNullOrWhiteSpace(username) ? "system" : username;
        }

        private static string ResolveCompanyName(CompanySetting? company)
        {
            return string.IsNullOrWhiteSpace(company?.CompanyName) ? "Emeditech Plus LLP" : company.CompanyName.Trim();
        }

        private static string ReplaceBranding(string text, string companyName)
        {
            return text
                .Replace("Emeditech Plus LLP", companyName, StringComparison.OrdinalIgnoreCase)
                .Replace("EMEDITECH PLUS LLP", companyName.ToUpperInvariant(), StringComparison.Ordinal)
            .Replace("EMEDITECHPLUS LLP", companyName.ToUpperInvariant(), StringComparison.Ordinal)
            .Replace("Central Licence Policy", companyName, StringComparison.OrdinalIgnoreCase)
            .Replace("Central Licence", companyName, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReplacePlaceholders(string text, Dictionary<string, string> placeholders)
        {
            foreach (var kv in placeholders)
                text = text.Replace($"{{{{{kv.Key}}}}}", kv.Value, StringComparison.OrdinalIgnoreCase);
            return text;
        }

        private static string? ResolveLocalDomain(MailConfiguration config)
        {
            if (!string.IsNullOrWhiteSpace(config.FromEmail) && config.FromEmail.Contains('@'))
            {
                var domain = config.FromEmail.Split('@').Last().Trim();
                if (!string.IsNullOrWhiteSpace(domain)) return domain;
            }

            if (!string.IsNullOrWhiteSpace(config.SmtpUsername) && config.SmtpUsername.Contains('@'))
            {
                var domain = config.SmtpUsername.Split('@').Last().Trim();
                if (!string.IsNullOrWhiteSpace(domain)) return domain;
            }

            if (!string.IsNullOrWhiteSpace(config.SmtpServer))
            {
                return config.SmtpServer.Trim();
            }

            return null;
        }

        private static SecureSocketOptions ResolveSecureSocketOptions(int port, bool enableSsl)
        {
            if (!enableSsl)
            {
                return SecureSocketOptions.None;
            }

            return port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                25  => SecureSocketOptions.StartTlsWhenAvailable,
                _   => SecureSocketOptions.Auto
            };
        }
    }
}
