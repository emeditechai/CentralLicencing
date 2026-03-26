using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Models;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IMailConfigRepository _mailConfigRepo;
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly ICompanySettingsRepository _companySettingsRepo;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IMailConfigRepository mailConfigRepo,
            IEmailTemplateRepository templateRepo,
            ICompanySettingsRepository companySettingsRepo,
            ILogger<SmtpEmailService> logger)
        {
            _mailConfigRepo = mailConfigRepo;
            _templateRepo   = templateRepo;
            _companySettingsRepo = companySettingsRepo;
            _logger         = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            var config = await _mailConfigRepo.GetActiveAsync();
            if (config == null)
            {
                _logger.LogWarning("No active mail configuration found. Email not sent to {Email}.", toEmail);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}.", toEmail);
            }
        }

        public async Task SendTemplatedAsync(string templateKey, string toEmail, string toName,
            Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            var template = await _templateRepo.GetByKeyAsync(templateKey);
            if (template == null)
            {
                _logger.LogWarning("Email template '{Key}' not found or inactive.", templateKey);
                return;
            }

            var company = await _companySettingsRepo.GetParentCompanyAsync();
            var companyName = ResolveCompanyName(company);
            placeholders["CompanyName"] = companyName;
            placeholders["AppName"] = companyName;

            var subject = ReplaceBranding(ReplacePlaceholders(template.Subject, placeholders), companyName);
            var body = ReplaceBranding(ReplacePlaceholders(template.Body, placeholders), companyName);

            await SendAsync(toEmail, toName, subject, body);
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
