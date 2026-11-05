using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CentralLicenceApp.Models
{
    public static class EnquiryStageCodes
    {
        public const string New = "NEW";
        public const string Contacted = "CONTACTED";
        public const string DemoScheduled = "DEMO_SCHEDULED";
        public const string DemoDone = "DEMO_DONE";
        public const string QuotationSent = "QUOTATION_SENT";
        public const string Won = "WON";
        public const string Lost = "LOST";
    }

    /// <summary>Which enquiries a user may see: everything, or their own and their team's.</summary>
    public class EnquiryScope
    {
        public bool All { get; set; }
        public int UserId { get; set; }
        public List<int> UserIds { get; set; } = new();
        public bool CanManageTeam => All || UserIds.Count > 1;
    }

    public class EnquiryLookup
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EnquiryStage : EnquiryLookup
    {
        public string? StageCode { get; set; }
        public int DefaultFollowUpDays { get; set; } = 2;
        public bool IsClosed { get; set; }
        public string ColorCode { get; set; } = "#6366f1";
    }

    public class EnquiryCallOutcome : EnquiryLookup
    {
        public bool IsConnected { get; set; }
        public int? RetryHours { get; set; }
    }

    public class EnquiryClient
    {
        public int Id { get; set; }
        public string? ClientCode { get; set; }
        [Required(ErrorMessage = "Company name is required."), MaxLength(200), Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;
        public int? BusinessTypeId { get; set; }
        [MaxLength(20)] public string? GSTIN { get; set; }
        [MaxLength(500)] public string? Address { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(100)] public string? State { get; set; }
        [MaxLength(200)] public string? Website { get; set; }
        [MaxLength(200)] public string? ExistingSoftware { get; set; }
        [MaxLength(100)] public string? SizeInfo { get; set; }
        [MaxLength(200)] public string? ReferredBy { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public string? LinkedClientCode { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? BusinessTypeName { get; set; }
        public string? PrimaryContactName { get; set; }
        public string? PrimaryMobile { get; set; }
        public string? PrimaryEmail { get; set; }
        public int EnquiryCount { get; set; }
        public int OpenEnquiryCount { get; set; }
        public List<EnquiryClientContact> Contacts { get; set; } = new();
    }

    public class EnquiryClientContact
    {
        public int Id { get; set; }
        public int EnquiryClientId { get; set; }
        [MaxLength(150)] public string Name { get; set; } = string.Empty;
        [MaxLength(100)] public string? Role { get; set; }
        [MaxLength(20)] public string? Mobile { get; set; }
        [MaxLength(20)] public string? WhatsApp { get; set; }
        [MaxLength(200)] public string? Email { get; set; }
        public bool IsPrimary { get; set; }
        public bool WhatsAppOptOut { get; set; }
    }

    public class EnquiryDuplicateMatch
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? MatchedOn { get; set; }
    }

    public class Enquiry
    {
        public int Id { get; set; }
        public string? EnquiryNo { get; set; }
        public int EnquiryClientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Requirement { get; set; }
        public int? SourceId { get; set; }
        public int StageId { get; set; }
        public int? PriorityId { get; set; }
        public int OwnerUserId { get; set; }
        public decimal? ExpectedValue { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? NextFollowUpAt { get; set; }
        public int? LostReasonId { get; set; }
        public string? LostRemarks { get; set; }
        public int? QuotationId { get; set; }
        public bool IsOnHold { get; set; }
        public DateTime? ResumeDate { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string? PrimaryContactName { get; set; }
        public string? PrimaryMobile { get; set; }
        public string? SourceName { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string? StageCode { get; set; }
        public string? StageColor { get; set; }
        public bool StageIsClosed { get; set; }
        public string? PriorityName { get; set; }
        public string? PriorityColor { get; set; }
        public string? OwnerName { get; set; }
        public string? LostReasonName { get; set; }
        public string? QuotationNo { get; set; }
        public string? ProductNames { get; set; }
    }

    public class EnquiryFollowUp
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public string FollowUpType { get; set; } = "Call";
        public DateTime DueAt { get; set; }
        public int AssignedToUserId { get; set; }
        public string? Purpose { get; set; }
        public string Status { get; set; } = "Open";
        public string? Result { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int RescheduleCount { get; set; }
        public string? LastRescheduleReason { get; set; }
        public string? AssignedToName { get; set; }
    }

    public class EnquiryCallLog
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public int? ContactId { get; set; }
        public int? FollowUpId { get; set; }
        public string Direction { get; set; } = "Outgoing";
        public string? PhoneNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int DurationSeconds { get; set; }
        public int OutcomeId { get; set; }
        public string? Notes { get; set; }
        public int CalledByUserId { get; set; }
        public string? OutcomeName { get; set; }
        public string? CalledByName { get; set; }
        public string? ContactName { get; set; }
        public string? CompanyName { get; set; }
        public string? EnquiryNo { get; set; }
    }

    public class EnquiryDemo
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public int ProductId { get; set; }
        public string Mode { get; set; } = "Online";
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public int PresenterUserId { get; set; }
        public string? MeetingLink { get; set; }
        public string? Attendees { get; set; }
        public string Status { get; set; } = "Scheduled";
        public int? Rating { get; set; }
        public string? InterestLevel { get; set; }
        public string? Feedback { get; set; }

        public string? ProductName { get; set; }
        public string? PresenterName { get; set; }
        public string? EnquiryNo { get; set; }
        public string? CompanyName { get; set; }
        public int EnquiryClientId { get; set; }
        public List<int> AttendeeContactIds { get; set; } = new();
    }

    public class EnquiryActivity
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EnquiryAttachment
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredName { get; set; } = string.Empty;
        public string? UploadedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EnquiryCallListItem
    {
        public int FollowUpId { get; set; }
        public int EnquiryId { get; set; }
        public string? EnquiryNo { get; set; }
        public string FollowUpType { get; set; } = "Call";
        public DateTime DueAt { get; set; }
        public string? Purpose { get; set; }
        public int RescheduleCount { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public int? ContactId { get; set; }
        public string? Mobile { get; set; }
        public string? WhatsApp { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string? StageCode { get; set; }
        public string? StageColor { get; set; }
        public string? PriorityName { get; set; }
        public string? PriorityColor { get; set; }
        public string? ProductNames { get; set; }
        public string? AssignedToName { get; set; }
        public int Attempts { get; set; }
        public string? LastCallSummary { get; set; }
        public DateTime? LastCallAt { get; set; }
    }

    public class EnquiryFilter
    {
        public string? Search { get; set; }
        public int? StageId { get; set; }
        public int? SourceId { get; set; }
        public int? PriorityId { get; set; }
        public int? ProductId { get; set; }
        public int? OwnerId { get; set; }
        public string? State { get; set; }   // open, closed, overdue, hold
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class EnquiryCountRow
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int Total { get; set; }
        public int Won { get; set; }
        public decimal Value { get; set; }
    }

    public class EnquiryUserActivityRow
    {
        public string UserName { get; set; } = string.Empty;
        public int OpenEnquiries { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public int Calls { get; set; }
        public int ConnectedCalls { get; set; }
        public int FollowUpsDone { get; set; }
        public int Overdue { get; set; }
        public int Demos { get; set; }
    }

    public class EnquiryDashboardSummary
    {
        public int OpenEnquiries { get; set; }
        public int DueToday { get; set; }
        public int Overdue { get; set; }
        public int UpcomingDemos { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public decimal OpenValue { get; set; }
        public int ConversionPercent => Won + Lost == 0 ? 0 : (int)Math.Round(100.0 * Won / (Won + Lost));
    }

    public class EnquiryReminderItem
    {
        public int RefId { get; set; }
        public int EnquiryId { get; set; }
        public string? EnquiryNo { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Mobile { get; set; }
        public DateTime DueAt { get; set; }
        public string? Detail { get; set; }
        public int UserId { get; set; }
        public int? ManagerId { get; set; }
        public string? UserName { get; set; }
    }

    public class EnquiryDigestRow
    {
        public int UserId { get; set; }
        public int DueToday { get; set; }
        public int Overdue { get; set; }
    }

    public class EnquiryNotifyUser
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class EnquiryQuotationOption
    {
        public int Id { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
