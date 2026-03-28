using System;
using System.Collections.Generic;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ClientDetailsReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ProductType { get; set; }
        public List<string> ProductTypes { get; set; } = new();
        public List<ClientDetailsReportRow> Items { get; set; } = new();
        public int TotalClients => Items.Count;
        public int InternalUseCount => Items.Count(x => x.IsInternalUse);
        public int ReferencedClientCount => Items.Count(x => !string.IsNullOrWhiteSpace(x.ReferenceClientCode));
    }
}