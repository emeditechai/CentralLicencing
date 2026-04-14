using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IHelpDeskTicketRepository
    {
        // Ticket CRUD
        Task<IEnumerable<HelpDeskTicket>> GetAllAsync();
        Task<IEnumerable<HelpDeskTicket>> GetByCreatorAsync(int userId);
        Task<IEnumerable<HelpDeskTicket>> GetByAssigneeAsync(int userId);
        /// <summary>
        /// Returns tickets currently assigned to the user OR previously assigned (found via audit log).
        /// </summary>
        Task<IEnumerable<HelpDeskTicket>> GetTicketsForAgentAsync(int userId);
        Task<HelpDeskTicket?> GetByIdAsync(int id);
        Task<int> CreateAsync(HelpDeskTicket ticket);
        Task<bool> UpdateStatusAsync(int ticketId, string status, DateTime? resolvedAt, DateTime? closedAt);
        Task<bool> AssignAsync(int ticketId, int agentId);
        Task<bool> SetFirstResponseAsync(int ticketId);
        Task<string> GenerateTicketNumberAsync();

        // Messages
        Task<IEnumerable<TicketMessage>> GetMessagesAsync(int ticketId);
        Task<int> AddMessageAsync(TicketMessage message);

        // Attachments
        Task<IEnumerable<TicketAttachment>> GetAttachmentsAsync(int ticketId);
        Task<TicketAttachment?> GetAttachmentByIdAsync(int id);
        Task<int> AddAttachmentAsync(TicketAttachment attachment);

        // Audit log
        Task<IEnumerable<TicketAuditLog>> GetAuditLogsAsync(int ticketId);
        Task<int> AddAuditLogAsync(TicketAuditLog log);

        // Agents
        Task<IEnumerable<AgentOption>> GetAgentsAsync();
    }
}
