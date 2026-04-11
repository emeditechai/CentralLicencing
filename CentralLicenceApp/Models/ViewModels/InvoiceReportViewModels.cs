using System;
using System.Collections.Generic;
using System.Linq;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Models.ViewModels
{
    public class DailyCollectionReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<DailyCollectionRow> Items { get; set; } = new();
        public bool IsAdminView { get; set; }

        /// <summary>Admin-only: selected user filter (username).</summary>
        public string? CollectedByFilter { get; set; }

        /// <summary>Admin-only: list of usernames that have collected payments (for the dropdown).</summary>
        public List<string> CollectedByUsers { get; set; } = new();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Summary stats
        public int TotalCollections => TotalCount;
        public decimal TotalAmountCollected => Items.Sum(x => x.TotalAmountPaid);
        public int UniqueInvoices => Items.Select(x => x.InvoiceNo).Distinct().Count();
    }

    public class ClientDueReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<ClientDueRow> Items { get; set; } = new();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Summary stats
        public int TotalOutstandingInvoices => TotalCount;
        public decimal TotalBalanceDue => Items.Sum(x => x.BalanceDue);
        public int OverdueCount => Items.Count(x => x.OverdueDays > 0);
    }
}
