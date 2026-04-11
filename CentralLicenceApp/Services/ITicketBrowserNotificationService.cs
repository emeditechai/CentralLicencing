using CentralLicenceApp.Models;

namespace CentralLicenceApp.Services
{
    public interface ITicketBrowserNotificationService
    {
        Task NotifyTicketCreatedAsync(HelpDeskTicket ticket);
        Task NotifyTicketAssignedAsync(HelpDeskTicket ticket, string assigneeName, int assigneeId);
        Task NotifyStatusChangedAsync(HelpDeskTicket ticket, string oldStatus, string newStatus);
        Task NotifyNewReplyAsync(HelpDeskTicket ticket, string replierName, string messageSnippet, bool isInternal);
    }
}
