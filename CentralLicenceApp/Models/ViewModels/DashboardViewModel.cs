using System.Collections.Generic;

namespace CentralLicenceApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public int ExpiringThisMonth { get; set; }
        public int TotalValidations { get; set; }
        public int FailedValidations { get; set; }
        public int TodayValidations { get; set; }
        public int ApprovedExpenseRequests { get; set; }
        public int ReimbursementInProcessRequests { get; set; }
        public int SettledExpenseRequests { get; set; }
        public string? SelectedProductType { get; set; }
        public List<string> AvailableProductTypes { get; set; } = new();
        public List<ClientAppLicense> RecentClients { get; set; } = new();
        public List<ClientAppLicense> UpcomingExpiries { get; set; } = new();
        public List<MonthlyValidationStat> MonthlyStats { get; set; } = new();
        public List<ValidationStatusStat> ValidationStatusStats { get; set; } = new();
    }

    public class MonthlyValidationStat
    {
        public string Month { get; set; } = string.Empty;
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
    }

    public class ValidationStatusStat
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
