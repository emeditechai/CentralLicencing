using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Services
{
    public interface IDailyCollectionExportService
    {
        byte[] GenerateExcel(IReadOnlyList<DailyCollectionRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
        byte[] GeneratePdf(IReadOnlyList<DailyCollectionRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
    }
}
