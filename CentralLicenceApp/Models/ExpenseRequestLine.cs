using System;

namespace CentralLicenceApp.Models
{
    public class ExpenseRequestLine
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string ItemType { get; set; } = "Expense";
        public int? ExpenseCategoryId { get; set; }
        public string? ExpenseCategoryName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ProjectOrCostCenter { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal Amount { get; set; }
        public decimal? PayableAmountInr { get; set; }
        public string? AccommodationCountry { get; set; }
        public string? AccommodationCity { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? ReceiptPath { get; set; }
        public List<ExpenseRequestLineAttachment> Attachments { get; set; } = new();
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}