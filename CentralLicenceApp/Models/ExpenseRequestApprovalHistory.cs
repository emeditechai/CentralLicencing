using System;

namespace CentralLicenceApp.Models
{
    public class ExpenseRequestApprovalHistory
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string ActionTaken { get; set; } = string.Empty;
        public int? ActionByUserId { get; set; }
        public string ActionByName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime ActionAt { get; set; }
    }
}