using System;
using System.Collections.Generic;
using System.Linq;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ExpenseReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<ExpenseReportRow> Items { get; set; } = new();
        public bool IsAdminView { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Summary stats across ALL pages (from SP's TotalCount + per-page aggregates)
        public int TotalRequests => TotalCount;
        public decimal TotalAmount => Items.Sum(x => x.TotalAmount);
        public int SettledCount => Items.Count(x => x.SettledAt.HasValue);
    }

    public class SettlementReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SettlementReportRow> Items { get; set; } = new();
        public bool IsAdminView { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public int TotalSettlements => TotalCount;
        public decimal TotalSettlementAmount => Items.Sum(x => x.SettlementAmount ?? 0);
        public decimal TotalRequestedAmount => Items.Sum(x => x.TotalAmount);
    }
}
