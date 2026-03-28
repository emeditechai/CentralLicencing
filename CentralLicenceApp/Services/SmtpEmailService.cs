using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            await SendCoreAsync(string.IsNullOrWhiteSpace(emailType) ? "Direct Email" : emailType, null, toEmail, toName, subject, htmlBody);
        }

        private async Task SendCoreAsync(string emailType, string? templateKey, string? toEmail, string? toName,
            string? subject, string? htmlBody)
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
#pragma warning disable CA1416
                using var client = new SmtpClient(config.SmtpServer, config.SmtpPort)
                {
                    EnableSsl   = config.EnableSSL,
                    Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword)
                };

                using var mail = new MailMessage
                {
                    From       = new MailAddress(config.FromEmail, config.FromName),
                    Subject    = subject,
                    Body       = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(mail);
#pragma warning restore CA1416
                _logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
                await LogEmailAsync(emailType, templateKey, toEmail, toName, subject, htmlBody, "Sent", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}.", toEmail);
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

            await SendCoreAsync(template.TemplateName, template.TemplateKey, toEmail, toName, subject, body);
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
    }
}
