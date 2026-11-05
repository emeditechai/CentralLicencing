using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = AccessRoles)]
    public class EnquiryDemoController : EnquiryControllerBase
    {
        private readonly IEnquiryRepository _repo;
        private readonly IProductMasterRepository _productRepo;
        private readonly IEnquiryNotificationService _notifier;

        public EnquiryDemoController(IEnquiryRepository repo, IProductMasterRepository productRepo, IEnquiryNotificationService notifier,
            IUserRepository userRepo, IPermissionService permissionService) : base(userRepo, permissionService)
        {
            _repo = repo;
            _productRepo = productRepo;
            _notifier = notifier;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var first = year is >= 2000 and <= 2100 && month is >= 1 and <= 12
                ? new DateTime(year.Value, month.Value, 1)
                : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return View(new EnquiryDemoCalendarViewModel
            {
                Month = first,
                Demos = await _repo.GetDemosAsync(await GetScopeAsync(), first.AddDays(-7), first.AddMonths(1).AddDays(7))
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var scope = await GetScopeAsync();
            var d = await _repo.GetDemoAsync(id, scope);
            if (d == null) return NotFound();
            ViewBag.Contacts = await _repo.GetDemoContactsAsync(id);
            return View(d);
        }

        private async Task<EnquiryDemoFormViewModel> BuildFormAsync(EnquiryDemo demo, EnquiryScope scope)
        {
            var enquiry = await _repo.GetEnquiryAsync(demo.EnquiryId, scope);
            var all = new EnquiryScope { All = true, UserId = scope.UserId };
            return new EnquiryDemoFormViewModel
            {
                Demo = demo,
                Enquiry = enquiry,
                Contacts = enquiry == null ? new() : (await _repo.GetClientAsync(enquiry.EnquiryClientId, all))?.Contacts ?? new(),
                Products = (await _productRepo.GetAllActiveAsync()).ToList(),
                Users = await GetAssignableUsersAsync(scope)
            };
        }

        [HttpGet]
        public async Task<IActionResult> Create(int enquiryId)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(enquiryId, scope) == null) return NotFound();
            var products = await _repo.GetEnquiryProductIdsAsync(enquiryId);
            return View("Form", await BuildFormAsync(new EnquiryDemo
            {
                EnquiryId = enquiryId,
                ProductId = products.FirstOrDefault(),
                PresenterUserId = scope.UserId,
                ScheduledAt = DateTime.Now.AddDays(2).Date.AddHours(11)
            }, scope));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var scope = await GetScopeAsync();
            var d = await _repo.GetDemoAsync(id, scope);
            if (d == null) return NotFound();
            return View("Form", await BuildFormAsync(d, scope));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnquiryDemoFormViewModel m) => await SaveAsync(m, 0);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnquiryDemoFormViewModel m)
        {
            var existing = await _repo.GetDemoAsync(id, await GetScopeAsync());
            if (existing == null) return NotFound();
            if (existing.Status is "Completed" or "Cancelled" or "NoShow")
            {
                TempData["Error"] = "This demo is closed. Schedule a new one instead.";
                return RedirectToAction(nameof(Details), new { id });
            }
            m.Demo.EnquiryId = existing.EnquiryId;
            return await SaveAsync(m, id, existing);
        }

        private async Task<IActionResult> SaveAsync(EnquiryDemoFormViewModel m, int id, EnquiryDemo? old = null)
        {
            var scope = await GetScopeAsync();
            var d = m.Demo;
            d.Id = id;
            var enquiry = await _repo.GetEnquiryAsync(d.EnquiryId, scope);
            if (enquiry == null) return NotFound();
            var presenters = await GetAssignableUsersAsync(scope);
            var product = (await _productRepo.GetAllActiveAsync()).FirstOrDefault(p => p.Id == d.ProductId);

            string? error = null;
            if (enquiry.StageIsClosed) error = "Reopen this enquiry before scheduling a demo.";
            else if (product == null) error = "Select a product.";
            else if (presenters.All(u => u.Id != d.PresenterUserId)) error = "Select a presenter from your team.";
            else if (d.ScheduledAt < DateTime.Now.AddMinutes(-5)) error = "Choose a demo time that isn't in the past.";
            else if (d.DurationMinutes is < 5 or > 600) error = "Enter a duration between 5 and 600 minutes.";
            else if (string.IsNullOrWhiteSpace(d.MeetingLink)) error = d.Mode == "OnSite" ? "Enter the address for the on-site demo." : "Enter the meeting link, so it can be sent to everyone attending.";
            if (error != null)
            {
                TempData["Error"] = error;
                return View("Form", await BuildFormAsync(d, scope));
            }

            d.Mode = d.Mode == "OnSite" ? "OnSite" : "Online";
            var clashes = await _repo.PresenterClashCountAsync(d.PresenterUserId, d.ScheduledAt, d.DurationMinutes, id);
            var savedId = await _repo.SaveDemoAsync(d, scope.UserId);
            var changed = old == null || old.ScheduledAt != d.ScheduledAt || old.MeetingLink != d.MeetingLink;
            if (changed)
                _notifier.QueueDemoInvitation(savedId, enquiry.CompanyName, product!.ProductName, d.ScheduledAt, d.MeetingLink,
                    old == null ? "this is your invitation" : "these are the updated details");
            // The host always gets the link, including when they booked the demo themselves.
            if (changed || old!.PresenterUserId != d.PresenterUserId)
                _notifier.QueueUserMessage(d.PresenterUserId, old == null ? "Demo scheduled: you are the host" : "Demo updated: you are the host",
                    $"{enquiry.CompanyName}: {product!.ProductName}\n{d.ScheduledAt:dd MMM yyyy hh:mm tt} ({d.DurationMinutes} minutes)\n\nMeeting link / address:\n{d.MeetingLink}", $"/EnquiryDemo/Details/{savedId}");
            TempData["Success"] = "Demo saved.";
            if (clashes > 0) TempData["Warning"] = $"This presenter already has {clashes} demo(s) at that time.";
            return RedirectToAction(nameof(Details), new { id = savedId });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Complete(int id, int? rating, string? interestLevel, string? feedback, DateTime? nextAt)
        {
            var scope = await GetScopeAsync();
            var d = await _repo.GetDemoAsync(id, scope);
            if (d == null) return NotFound();
            var enquiry = await _repo.GetEnquiryAsync(d.EnquiryId, scope);
            if (d.Status is "Completed" or "Cancelled" or "NoShow" || enquiry == null) { TempData["Error"] = "This demo is already closed."; return RedirectToAction(nameof(Details), new { id }); }
            if (nextAt == null || nextAt < DateTime.Now.AddMinutes(-5)) { TempData["Error"] = "Set the next follow-up, which is the next step after the demo."; return RedirectToAction(nameof(Details), new { id }); }
            if (rating is < 1 or > 5) rating = null;
            await _repo.CompleteDemoAsync(id, rating, interestLevel is "High" or "Medium" or "Low" ? interestLevel : null, feedback?.Trim(),
                new EnquiryFollowUp { DueAt = nextAt.Value, AssignedToUserId = enquiry.OwnerUserId, Purpose = "Follow up after demo" }, scope.UserId);
            TempData["Success"] = "Demo completed and next follow-up planned.";
            return RedirectToAction("Details", "ClientEnquiry", new { id = d.EnquiryId });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> CloseDemo(int id, string? status, string? reason)
        {
            var scope = await GetScopeAsync();
            var d = await _repo.GetDemoAsync(id, scope);
            if (d == null) return NotFound();
            if (status is not ("Cancelled" or "NoShow") || d.Status is "Completed" or "Cancelled" or "NoShow")
            {
                TempData["Error"] = "This demo can't be changed.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await _repo.CloseDemoAsync(id, status, reason?.Trim(), scope.UserId);
            TempData["Success"] = status == "NoShow" ? "Demo marked as a no-show." : "Demo cancelled.";
            return RedirectToAction("Details", "ClientEnquiry", new { id = d.EnquiryId });
        }
    }
}
