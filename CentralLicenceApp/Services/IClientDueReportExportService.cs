using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Services
{
    public interface IClientDueReportExportService
    {
        byte[] GenerateExcel(IReadOnlyList<ClientDueRow> items, string? fromDateLabel, string? toDateLabel);
        byte[] GeneratePdf(IReadOnlyList<ClientDueRow> items, string? fromDateLabel, string? toDateLabel);
    }
}
