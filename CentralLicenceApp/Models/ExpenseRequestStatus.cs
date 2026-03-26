namespace CentralLicenceApp.Models
{
    public static class ExpenseRequestStatus
    {
        public const string Draft = "Draft";
        public const string PendingApproval = "Pending Approval";
        public const string Approved = "Approved";
        public const string ReimbursementInProcess = "Reimbursement In Process";
        public const string Settled = "Settled";
        public const string Rejected = "Rejected";
    }
}