using System;

namespace CentralLicenceApp.Models.Reports
{
    public class ClientDetailsReportRow
    {
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? EmailID { get; set; }
        public string? ClientPersonName { get; set; }
        public string? Address { get; set; }
        public bool IsInternalUse { get; set; }
        public string? ReferenceClientCode { get; set; }
        public string PurchasedProductSummary { get; set; } = string.Empty;
        public DateTime LicenseStartDate { get; set; }
        public bool IsActive { get; set; }
    }
}