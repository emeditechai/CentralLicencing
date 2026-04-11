using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmailTemplateController : Controller
    {
        private readonly IEmailTemplateRepository _repo;
        private readonly IEmailService _emailService;

        public EmailTemplateController(IEmailTemplateRepository repo, IEmailService emailService)
        {
            _repo         = repo;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _repo.GetAllAsync();
            return View(templates);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _repo.GetByIdAsync(id);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CentralLicenceApp.Models.EmailTemplate vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            await _repo.UpdateAsync(vm);
            TempData["Success"] = $"Template <strong>{vm.TemplateName}</strong> updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTest(int id, string testEmail)
        {
            if (string.IsNullOrWhiteSpace(testEmail))
            {
                TempData["Error"] = "Please provide a test email address.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var template = await _repo.GetByIdAsync(id);
            if (template == null) return NotFound();

            var placeholders = new System.Collections.Generic.Dictionary<string, string>
            {
                // Licence / Client
                ["ClientName"]    = "Test Client",
                ["ClientCode"]    = "CI-000001",
                ["ExpiryDate"]    = System.DateTime.Today.AddMonths(1).ToString("dd MMM yyyy"),
                ["AmcExpiryDate"] = System.DateTime.Today.AddMonths(1).ToString("dd MMM yyyy"),
                ["DaysRemaining"] = "7",
                ["AppName"]       = "eRestoPOS",
                ["AppUrl"]        = "http://your-app-url",
                // Expense / Settlement
                ["RequestNumber"] = "EXP-20260326-0007",
                ["EmployeeName"] = "Abhik Porel",
                ["EmployeeCode"] = "EMP-104",
                ["PurposeOfTravel"] = "Client visit – travel and accommodation.",
                ["TotalAmount"] = "18,750.00",
                ["ItemCount"] = "4",
                ["SubmittedAt"] = System.DateTime.Now.ToString("dd MMM yyyy hh:mm tt"),
                ["CurrentStatus"] = "Pending Approval",
                ["ApproverName"] = "Operations Manager",
                ["DetailsUrl"] = "http://your-app-url/ExpenseRequest/Details/7",
                ["SettlementReceiptNumber"] = "SET-20260326-0007",
                ["SettlementDate"] = System.DateTime.Today.ToString("dd MMM yyyy"),
                ["SettlementAmount"] = "18,750.00",
                ["SettlementMode"] = "Bank Transfer",
                ["SettlementReferenceNo"] = "UTR0045892361",
                ["SettlementReceiptUrl"] = "http://your-app-url/ExpenseRequest/SettlementReceipt/7",
                // Password Reset
                ["FullName"] = "Abhik Porel",
                ["ResetUrl"] = "http://your-app-url/Account/ResetPassword?token=sample",
                // Ticket
                ["TicketNumber"] = "TKT-20260326-0001",
                ["Heading"] = "New Support Ticket Created",
                ["IntroMessage"] = "A new support ticket has been submitted. Please find the details below.",
                ["DetailsTable"] = "<table style=\"width:100%;border-collapse:collapse;\"><tr><td style=\"padding:6px 10px;border:1px solid #e5e7eb;\">Subject</td><td style=\"padding:6px 10px;border:1px solid #e5e7eb;\">Login issue</td></tr></table>",
                ["NewStatus"] = "In Progress",
                ["NoteLabel"] = "Public Reply",
                // Quotation
                ["QuotationNo"] = "QT-250001",
                ["QuotationDate"] = System.DateTime.Today.ToString("dd MMM yyyy"),
                ["PartyName"] = "RAJ Traders",
                ["LineItemsTable"] = "<table style=\"width:100%;border-collapse:collapse;\"><tr style=\"background:#f1f5f9;\"><th style=\"padding:6px 10px;border:1px solid #e5e7eb;\">Item</th><th style=\"padding:6px 10px;border:1px solid #e5e7eb;\">Amount</th></tr><tr><td style=\"padding:6px 10px;border:1px solid #e5e7eb;\">Product A</td><td style=\"padding:6px 10px;border:1px solid #e5e7eb;\">₹10,000</td></tr></table>",
                ["TotalsTable"] = "<table style=\"width:100%;\"><tr><td style=\"padding:4px 10px;\"><strong>Total</strong></td><td style=\"padding:4px 10px;text-align:right;\"><strong>₹10,000.00</strong></td></tr></table>",
                ["ValidUntilInfo"] = "<p style=\"color:#64748b;font-size:13px;\">Valid until: " + System.DateTime.Today.AddDays(30).ToString("dd MMM yyyy") + "</p>",
                // Invoice
                ["InvoiceNo"] = "INV-250001",
                ["InvoiceDate"] = System.DateTime.Today.ToString("dd MMM yyyy"),
                ["DueInfo"] = "<p style=\"color:#64748b;font-size:13px;\">Due: " + System.DateTime.Today.AddDays(30).ToString("dd MMM yyyy") + " &bull; Balance Due: ₹10,000.00</p>",
                // User Onboarding
                ["Username"] = "johndoe",
                ["Email"] = "johndoe@example.com",
                ["PhoneNumber"] = "9876543210",
                ["RoleName"] = "Employee",
                ["LocationName"] = "Head Office",
                ["DepartmentName"] = "Operations",
                ["DesignationName"] = "Executive",
                ["ManagerName"] = "Abhik Porel",
                ["IsCoreMember"] = "No",
                ["Status"] = "Active",
                ["LoginUrl"] = "http://your-app-url/Account/Login",
                ["TemporaryPassword"] = "TempPass@123"
            };

            await _emailService.SendTemplatedAsync(template.TemplateKey, testEmail, "Test", placeholders);
            TempData["Success"] = $"Test email sent to <strong>{testEmail}</strong>.";
            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}
