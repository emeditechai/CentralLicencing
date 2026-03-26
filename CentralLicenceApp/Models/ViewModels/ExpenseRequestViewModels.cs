using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CentralLicenceApp.Models;
using Microsoft.AspNetCore.Http;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ExpenseRequestListPageViewModel
    {
        public List<ExpenseRequest> Requests { get; set; } = new();
        public string PageTitle { get; set; } = "Expense & Advance Requests";
        public string EmptyStateTitle { get; set; } = "No requests found";
        public string EmptyStateDescription { get; set; } = "Create a request to start the approval workflow.";
        public bool ShowCreateButton { get; set; }
        public bool ShowFinanceActions { get; set; }
    }

    public class ExpenseRequestFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Purpose of advance / travel is required")]
        [Display(Name = "Purpose of Advance / Travel")]
        [StringLength(200)]
        public string PurposeOfTravel { get; set; } = string.Empty;

        [Display(Name = "Note or comment for approvers")]
        [StringLength(500)]
        public string? EmployeeRemarks { get; set; }
    }

    public class ExpenseRequestLineFormViewModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [Display(Name = "Transaction Type")]
        public string ItemType { get; set; } = "Expense";

        [Display(Name = "Expense Category")]
        public int? ExpenseCategoryId { get; set; }

        [Required(ErrorMessage = "Expense title is required")]
        [Display(Name = "Expense Title")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Project / Cost Center")]
        [StringLength(120)]
        public string? ProjectOrCostCenter { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        [Display(Name = "Expense Date")]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        [StringLength(10)]
        public string CurrencyCode { get; set; } = "INR";

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 99999999)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Payable Amount - INR")]
        [Range(0, 99999999)]
        public decimal? PayableAmountInr { get; set; }

        [Display(Name = "Accommodation Country")]
        [StringLength(100)]
        public string? AccommodationCountry { get; set; }

        [Display(Name = "Accommodation City")]
        [StringLength(100)]
        public string? AccommodationCity { get; set; }

        [Display(Name = "Check-in")]
        [DataType(DataType.Date)]
        public DateTime? CheckInDate { get; set; }

        [Display(Name = "Check-out")]
        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }

        [Display(Name = "Receipts")]
        public List<IFormFile> ReceiptFiles { get; set; } = new();

        public List<ExpenseRequestLineAttachment> ExistingAttachments { get; set; } = new();

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        public List<ExpenseCategoryMaster> ExpenseCategories { get; set; } = new();
        public List<string> AvailableCurrencies { get; set; } = new();
        public List<string> AvailableCountries { get; set; } = new();
        public List<string> AvailableItemTypes { get; set; } = new();
    }

    public class ExpenseRequestManageViewModel
    {
        public ExpenseRequest Request { get; set; } = new();
        public ExpenseRequestFormViewModel RequestForm { get; set; } = new();
        public ExpenseRequestLineFormViewModel NewLine { get; set; } = new();
        public List<ExpenseRequestLine> Lines { get; set; } = new();
        public List<ExpenseRequestApprovalHistory> History { get; set; } = new();
        public ReimbursementStartViewModel ReimbursementForm { get; set; } = new();
        public ExpenseSettlementViewModel SettlementForm { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool IsEmployeeOwner { get; set; }
        public bool CanStartReimbursement { get; set; }
        public bool CanSettle { get; set; }
        public bool CanViewSettlementReceipt { get; set; }
    }

    public class ExpenseApprovalDecisionViewModel
    {
        public int RequestId { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class ReimbursementStartViewModel
    {
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Reimbursement remarks are required")]
        [Display(Name = "Reimbursement Remarks")]
        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;
    }

    public class ExpenseSettlementViewModel
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public decimal RequestTotalAmount { get; set; }

        [Required(ErrorMessage = "Settlement date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Settlement Date")]
        public DateTime? SettlementDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Settlement amount is required")]
        [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "Settlement amount must be greater than zero")]
        [Display(Name = "Settled Amount")]
        public decimal? SettlementAmount { get; set; }

        [Required(ErrorMessage = "Payment mode is required")]
        [Display(Name = "Payment Mode")]
        [StringLength(30)]
        public string SettlementMode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment reference is required")]
        [Display(Name = "Payment Reference No.")]
        [StringLength(100)]
        public string SettlementReferenceNo { get; set; } = string.Empty;

        [Display(Name = "Settlement Remarks")]
        [StringLength(500)]
        public string? SettlementRemarks { get; set; }

        public List<string> AvailablePaymentModes { get; set; } = new();
    }
}