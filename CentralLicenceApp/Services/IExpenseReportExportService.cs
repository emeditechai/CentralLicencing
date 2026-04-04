using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Services
{
    public interface IExpenseReportExportService
    {
        byte[] GenerateExcel(IReadOnlyList<ExpenseReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
        byte[] GeneratePdf(IReadOnlyList<ExpenseReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel);
    }
}
