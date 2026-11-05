using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEnquiryRepository
    {
        Task EnsureTablesAsync();

        // Masters
        Task<List<EnquiryLookup>> GetLookupsAsync(string table, bool activeOnly);
        Task<List<EnquiryStage>> GetStagesAsync(bool activeOnly);
        Task<List<EnquiryCallOutcome>> GetOutcomesAsync(bool activeOnly);
        Task SaveLookupAsync(string table, EnquiryLookup item);
        Task SaveStageAsync(EnquiryStage item);
        Task SaveOutcomeAsync(EnquiryCallOutcome item);

        // Enquiry Client Master
        Task<List<EnquiryClient>> GetClientsAsync(EnquiryScope scope, string? search);
        Task<EnquiryClient?> GetClientAsync(int id, EnquiryScope scope);
        Task<int> SaveClientAsync(EnquiryClient client);
        Task<List<EnquiryDuplicateMatch>> FindDuplicatesAsync(int excludeId, string? gstin, IEnumerable<string> mobiles, IEnumerable<string> emails);
        Task<bool> SetClientActiveAsync(int id, bool active);

        // Enquiries
        Task<List<Enquiry>> GetEnquiriesAsync(EnquiryScope scope, EnquiryFilter filter);
        Task<Enquiry?> GetEnquiryAsync(int id, EnquiryScope scope);
        Task<List<int>> GetEnquiryProductIdsAsync(int id);
        Task<List<int>> GetCollaboratorIdsAsync(int id);
        Task<int> CreateEnquiryAsync(Enquiry e, IEnumerable<int> productIds, IEnumerable<int> collaboratorIds, EnquiryFollowUp first, int userId);
        Task UpdateEnquiryAsync(Enquiry e, IEnumerable<int> productIds, IEnumerable<int> collaboratorIds, int userId);
        Task ChangeStageAsync(int id, EnquiryStage stage, int userId, DateTime? nextFollowUpAt, int? lostReasonId, string? lostRemarks, string? linkedClientCode);
        Task ReassignAsync(int id, int newOwnerId, string newOwnerName, string reason, int userId);
        Task SetHoldAsync(int id, DateTime? resumeDate, string? reason, int userId);
        Task SoftDeleteAsync(int id, int userId);
        Task AddNoteAsync(int id, string note, int userId);
        Task LinkQuotationAsync(int id, int quotationId, string quotationNo, int userId);

        // Follow-ups and calls
        Task<List<EnquiryFollowUp>> GetFollowUpsAsync(int enquiryId);
        Task<EnquiryFollowUp?> GetFollowUpAsync(int id);
        Task<int> AddFollowUpAsync(EnquiryFollowUp f, int userId);
        Task CompleteFollowUpAsync(int id, string result, EnquiryFollowUp? next, int userId);
        Task RescheduleFollowUpAsync(int id, DateTime newDue, string reason, int userId);
        Task<int> OpenFollowUpCountAsync(int enquiryId);
        Task LogCallAsync(EnquiryCallLog call, bool connected, EnquiryFollowUp? next, int userId);
        Task<List<EnquiryCallLog>> GetCallsAsync(int enquiryId);
        Task<List<EnquiryCallListItem>> GetCallListAsync(IEnumerable<int> userIds);

        // Demos
        Task<List<EnquiryDemo>> GetDemosAsync(EnquiryScope scope, DateTime from, DateTime to);
        Task<List<EnquiryDemo>> GetEnquiryDemosAsync(int enquiryId);
        Task<EnquiryDemo?> GetDemoAsync(int id, EnquiryScope scope);
        Task<int> SaveDemoAsync(EnquiryDemo d, int userId);
        Task<int> PresenterClashCountAsync(int presenterId, DateTime start, int minutes, int excludeDemoId);
        Task CompleteDemoAsync(int id, int? rating, string? interest, string? feedback, EnquiryFollowUp next, int userId);
        Task CloseDemoAsync(int id, string status, string? reason, int userId);

        // Timeline, attachments
        Task<List<EnquiryActivity>> GetActivitiesAsync(int enquiryId);
        Task<List<EnquiryAttachment>> GetAttachmentsAsync(int enquiryId);
        Task<EnquiryAttachment?> GetAttachmentAsync(int id);
        Task AddAttachmentAsync(EnquiryAttachment a, int userId);

        // Dashboard and reports
        Task<EnquiryDashboardSummary> GetSummaryAsync(EnquiryScope scope);
        Task<List<EnquiryCountRow>> GetPipelineAsync(EnquiryScope scope);
        Task<List<EnquiryCountRow>> GetBySourceAsync(EnquiryScope scope, DateTime? from, DateTime? to);
        Task<List<EnquiryCountRow>> GetLostReasonsAsync(EnquiryScope scope, DateTime? from, DateTime? to);
        Task<List<EnquiryCountRow>> GetCallOutcomesAsync(EnquiryScope scope, DateTime? from, DateTime? to);
        Task<List<EnquiryCountRow>> GetDemoOutcomesAsync(EnquiryScope scope, DateTime? from, DateTime? to);
        Task<List<EnquiryUserActivityRow>> GetUserActivityAsync(EnquiryScope scope, DateTime? from, DateTime? to);
        Task<List<EnquiryQuotationOption>> GetQuotationOptionsAsync();

        // Reminders
        Task<bool> TryClaimReminderAsync(string refType, int refId, string reminderType, int userId);
        Task<List<EnquiryReminderItem>> GetFollowUpsDueWithinAsync(int minutes);
        Task<List<EnquiryReminderItem>> GetOverdueForEscalationAsync(int hours);
        Task<List<EnquiryDigestRow>> GetDigestRowsAsync();
        Task<List<EnquiryReminderItem>> GetDemosStartingWithinAsync(int minutes);
        Task<EnquiryNotifyUser?> GetNotifyUserAsync(int userId);
        Task<List<EnquiryClientContact>> GetDemoContactsAsync(int demoId);
        Task<string?> GetDemoOtherAttendeesAsync(int demoId);
        Task<int> ResumeHeldEnquiriesAsync();
    }
}
