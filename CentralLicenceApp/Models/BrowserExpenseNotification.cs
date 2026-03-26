namespace CentralLicenceApp.Models
{
    public class BrowserExpenseNotification
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = "expense-request-submitted";
        public string Title { get; set; } = "Expense request submitted";
        public string Message { get; set; } = "A new expense or advance request requires attention.";
        public string TargetUrl { get; set; } = "/Dashboard/Index";
        public bool RequireDocumentHidden { get; set; } = true;
        public string RequestNumber { get; set; } = string.Empty;
    }
}