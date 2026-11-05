namespace CentralLicenceApp.Models.ViewModels
{
    public class EnquiryOptions
    {
        public List<EnquiryStage> Stages { get; set; } = new();
        public List<EnquiryLookup> Sources { get; set; } = new();
        public List<TicketPriorityMaster> Priorities { get; set; } = new();
        public List<ProductMaster> Products { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public List<EnquiryClient> Clients { get; set; } = new();
    }

    public class EnquiryIndexViewModel
    {
        public List<Enquiry> Enquiries { get; set; } = new();
        public EnquiryFilter Filter { get; set; } = new();
        public EnquiryOptions Options { get; set; } = new();
        public string ViewMode { get; set; } = "table";
        public bool CanManageTeam { get; set; }
    }

    public class EnquiryFormViewModel
    {
        public Enquiry Enquiry { get; set; } = new();
        public List<int> ProductIds { get; set; } = new();
        public List<int> CollaboratorIds { get; set; } = new();
        public DateTime? FirstFollowUpAt { get; set; }
        public string FirstFollowUpType { get; set; } = "Call";
        public string? FirstFollowUpPurpose { get; set; }
        public EnquiryOptions Options { get; set; } = new();
        public bool CanChooseOwner { get; set; }
    }

    public class EnquiryDetailsViewModel
    {
        public Enquiry Enquiry { get; set; } = new();
        public EnquiryClient? Client { get; set; }
        public List<EnquiryActivity> Activities { get; set; } = new();
        public List<EnquiryFollowUp> FollowUps { get; set; } = new();
        public List<EnquiryCallLog> Calls { get; set; } = new();
        public List<EnquiryDemo> Demos { get; set; } = new();
        public List<EnquiryAttachment> Attachments { get; set; } = new();
        public List<EnquiryStage> Stages { get; set; } = new();
        public List<EnquiryCallOutcome> Outcomes { get; set; } = new();
        public List<EnquiryLookup> LostReasons { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public List<EnquiryQuotationOption> Quotations { get; set; } = new();
        public List<string> ClientCodes { get; set; } = new();
        public bool CanManageTeam { get; set; }
    }

    public class EnquiryCallListViewModel
    {
        public List<EnquiryCallListItem> Overdue { get; set; } = new();
        public List<EnquiryCallListItem> Today { get; set; } = new();
        public List<EnquiryCallListItem> Upcoming { get; set; } = new();
        public List<EnquiryCallListItem> NewOnes { get; set; } = new();
        public List<EnquiryCallOutcome> Outcomes { get; set; } = new();
        public List<UserMaster> TeamUsers { get; set; } = new();
        public int? UserFilter { get; set; }
        public string Tab { get; set; } = "overdue";
    }

    public class EnquiryDashboardViewModel
    {
        public EnquiryDashboardSummary Summary { get; set; } = new();
        public List<EnquiryCountRow> Pipeline { get; set; } = new();
        public List<EnquiryCountRow> LostReasons { get; set; } = new();
        public List<EnquiryCallListItem> MyToday { get; set; } = new();
        public List<EnquiryDemo> UpcomingDemos { get; set; } = new();
    }

    public class EnquiryReportsViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<EnquiryCountRow> Funnel { get; set; } = new();
        public List<EnquiryCountRow> Sources { get; set; } = new();
        public List<EnquiryCountRow> LostReasons { get; set; } = new();
        public List<EnquiryCountRow> Calls { get; set; } = new();
        public List<EnquiryCountRow> Demos { get; set; } = new();
        public List<EnquiryUserActivityRow> Users { get; set; } = new();
    }

    public class EnquiryClientFormViewModel
    {
        public EnquiryClient Client { get; set; } = new();
        public List<EnquiryLookup> BusinessTypes { get; set; } = new();
        public List<EnquiryDuplicateMatch> Duplicates { get; set; } = new();
        public bool ConfirmDuplicate { get; set; }
        public bool ReturnToEnquiry { get; set; }
    }

    public class EnquiryDemoFormViewModel
    {
        public EnquiryDemo Demo { get; set; } = new();
        public Enquiry? Enquiry { get; set; }
        public List<EnquiryClientContact> Contacts { get; set; } = new();
        public List<ProductMaster> Products { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
    }

    public class EnquiryDemoCalendarViewModel
    {
        public DateTime Month { get; set; }
        public List<EnquiryDemo> Demos { get; set; } = new();
    }

    public class EnquiryMasterViewModel
    {
        public List<EnquiryStage> Stages { get; set; } = new();
        public List<EnquiryCallOutcome> Outcomes { get; set; } = new();
        public List<EnquiryLookup> Sources { get; set; } = new();
        public List<EnquiryLookup> BusinessTypes { get; set; } = new();
        public List<EnquiryLookup> LostReasons { get; set; } = new();
    }

    public class EnquiryMoveRequest
    {
        public int EnquiryId { get; set; }
        public int StageId { get; set; }
    }
}
