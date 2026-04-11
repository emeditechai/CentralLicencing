namespace CentralLicenceApp.Models
{
    public class BrowserTicketNotification
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = "ticket-created";
        public string Title { get; set; } = "Ticket notification";
        public string Message { get; set; } = "A ticket update requires your attention.";
        public string TargetUrl { get; set; } = "/HelpDeskTicket/MyTickets";
        public bool RequireDocumentHidden { get; set; } = true;
        public string TicketNumber { get; set; } = string.Empty;
    }
}
