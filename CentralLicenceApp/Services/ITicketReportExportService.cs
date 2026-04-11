using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Services
{
    public interface ITicketReportExportService
    {
        byte[] GenerateAgentPerformanceExcel(List<AgentPerformanceRow> items, string? fromDateLabel, string? toDateLabel);
        byte[] GenerateAgentPerformancePdf(List<AgentPerformanceRow> items, string? fromDateLabel, string? toDateLabel);
        byte[] GenerateSlaComplianceExcel(List<SlaComplianceRow> items, string? fromDateLabel, string? toDateLabel);
        byte[] GenerateSlaCompliancePdf(List<SlaComplianceRow> items, string? fromDateLabel, string? toDateLabel);
    }
}
