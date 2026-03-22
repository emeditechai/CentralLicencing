using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Services
{
    public class ExpiryReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiryReminderService> _logger;

        // Run the check at 9:00 AM every day
        private static readonly TimeSpan RunAt = TimeSpan.FromHours(9);

        public ExpiryReminderService(IServiceProvider serviceProvider,
            ILogger<ExpiryReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiryReminderService started.");

            // Initial delay until next scheduled run time
            var delay = GetDelayUntilNextRun(RunAt);
            _logger.LogInformation("Next expiry reminder run in {Minutes} minutes.", (int)delay.TotalMinutes);
            await Task.Delay(delay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running expiry reminder check...");
                    await ProcessRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during expiry reminder processing.");
                }

                // Wait 24 hours until the next run
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private static TimeSpan GetDelayUntilNextRun(TimeSpan targetTime)
        {
            var now = DateTime.Now;
            var next = now.Date + targetTime;
            if (now >= next)
                next = next.AddDays(1);
            return next - now;
        }

        private async Task ProcessRemindersAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var licenseRepo  = scope.ServiceProvider.GetRequiredService<IClientLicenseRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var reminderRepo = scope.ServiceProvider.GetRequiredService<IReminderRepository>();

            // ── 1. License Expiry Reminder (7 days) ──────────────────────────────
            var expiringLicenses = await licenseRepo.GetLicensesExpiringWithinDaysAsync(7);
            foreach (var license in expiringLicenses)
            {
                if (ct.IsCancellationRequested) return;
                if (string.IsNullOrWhiteSpace(license.EmailID)) continue;
                if (await reminderRepo.WasSentTodayAsync(license.Id, "LICENSE_EXPIRY_REMINDER")) continue;

                var days = (license.ExpiryDate.Date - DateTime.Today).Days;
                await emailService.SendTemplatedAsync(
                    "LICENSE_EXPIRY_REMINDER",
                    license.EmailID,
                    license.ClientName,
                    new Dictionary<string, string>
                    {
                        ["ClientName"]    = license.ClientName,
                        ["ClientCode"]    = license.ClientCode,
                        ["ExpiryDate"]    = license.ExpiryDate.ToString("dd MMM yyyy"),
                        ["DaysRemaining"] = days.ToString(),
                        ["AppName"]       = license.ProductType,
                        ["AppUrl"]        = license.AppUrl ?? string.Empty
                    });

                await reminderRepo.RecordAsync(license.Id, "LICENSE_EXPIRY_REMINDER", license.EmailID);
                _logger.LogInformation("LICENSE_EXPIRY_REMINDER sent to {Email} for {Code}.",
                    license.EmailID, license.ClientCode);
            }

            // ── 2. AMC Expiry Reminder (7 days) ──────────────────────────────────
            var amcExpiring = await licenseRepo.GetAmcExpiringWithinDaysAsync(7);
            foreach (var license in amcExpiring)
            {
                if (ct.IsCancellationRequested) return;
                if (string.IsNullOrWhiteSpace(license.EmailID)) continue;
                if (await reminderRepo.WasSentTodayAsync(license.Id, "AMC_EXPIRY_REMINDER")) continue;

                var days = license.AMC_Expireddate.HasValue
                    ? (license.AMC_Expireddate.Value.Date - DateTime.Today).Days
                    : 0;

                await emailService.SendTemplatedAsync(
                    "AMC_EXPIRY_REMINDER",
                    license.EmailID,
                    license.ClientName,
                    new Dictionary<string, string>
                    {
                        ["ClientName"]    = license.ClientName,
                        ["ClientCode"]    = license.ClientCode,
                        ["AmcExpiryDate"] = license.AMC_Expireddate?.ToString("dd MMM yyyy") ?? string.Empty,
                        ["DaysRemaining"] = days.ToString(),
                        ["AppName"]       = license.ProductType,
                        ["AppUrl"]        = license.AppUrl ?? string.Empty
                    });

                await reminderRepo.RecordAsync(license.Id, "AMC_EXPIRY_REMINDER", license.EmailID);
                _logger.LogInformation("AMC_EXPIRY_REMINDER sent to {Email} for {Code}.",
                    license.EmailID, license.ClientCode);
            }
        }
    }
}
