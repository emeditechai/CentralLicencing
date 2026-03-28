using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEmailLogRepository
    {
        Task<int> CreateAsync(EmailLogEntry entry);
        Task<(IEnumerable<EmailLogEntry> Items, int TotalCount)> GetPagedAsync(
            DateTime? fromDate, DateTime? toDate, string? emailType, int page, int pageSize);
        Task<IEnumerable<string>> GetDistinctEmailTypesAsync();
    }
}