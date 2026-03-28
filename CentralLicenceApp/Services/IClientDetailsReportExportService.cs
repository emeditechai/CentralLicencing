using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Services
{
    public interface IClientDetailsReportExportService
    {
        byte[] GenerateExcel(IReadOnlyList<ClientDetailsReportRow> items, string? productType, string? fromDateLabel, string? toDateLabel);
        byte[] GeneratePdf(IReadOnlyList<ClientDetailsReportRow> items, string? productType, string? fromDateLabel, string? toDateLabel);
    }
}