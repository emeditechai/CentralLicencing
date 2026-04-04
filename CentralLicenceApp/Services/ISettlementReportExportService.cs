using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Services
{
    public interface ISettlementReportExportService
    {
        byte[] GenerateExcel(IReadOnlyList<SettlementReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
        byte[] GeneratePdf(IReadOnlyList<SettlementReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
    }
}
