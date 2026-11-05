using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = AccessRoles)]
    public class ClientEnquiryController : EnquiryControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx" };
        private const long MaxFileBytes = 10 * 1024 * 1024;
        private static readonly string[] FollowUpTypes = { "Call", "WhatsApp", "Email", "Visit" };

        private readonly IEnquiryRepository _repo;
        private readonly ITicketPriorityRepository _priorityRepo;
        private readonly IProductMasterRepository _productRepo;
        private readonly IClientDetailsRepository _clientDetailsRepo;
        private readonly IEnquiryNotificationService _notifier;
        private readonly IWebHostEnvironment _env;

        public ClientEnquiryController(IEnquiryRepository repo, ITicketPriorityRepository priorityRepo, IProductMasterRepository productRepo,
            IClientDetailsRepository clientDetailsRepo, IEnquiryNotificationService notifier, IWebHostEnvironment env,
            IUserRepository userRepo, IPermissionService permissionService) : base(userRepo, permissionService)
        {
            _repo = repo;
            _priorityRepo = priorityRepo;
            _productRepo = productRepo;
            _clientDetailsRepo = clientDetailsRepo;
            _notifier = notifier;
            _env = env;
        }

        private static string NormalizeType(string? type) => FollowUpTypes.Contains(type) ? type! : "Call";

        private async Task<EnquiryOptions> BuildOptionsAsync(EnquiryScope scope) => new()
        {
            Stages = await _repo.GetStagesAsync(true),
            Sources = await _repo.GetLookupsAsync("EnquirySource", true),
            Priorities = (await _priorityRepo.GetAllActiveAsync()).OrderBy(p => p.SortOrder).ToList(),
            Products = (await _productRepo.GetAllActiveAsync()).ToList(),
            Users = await GetAssignableUsersAsync(scope),
            Clients = (await _repo.GetClientsAsync(scope, null)).Where(c => c.IsActive).ToList()
        };

        // ── Dashboard ──
        public async Task<IActionResult> Dashboard()
        {
            var scope = await GetScopeAsync();
            var callList = await _repo.GetCallListAsync(new[] { scope.UserId });
            return View(new EnquiryDashboardViewModel
            {
                Summary = await _repo.GetSummaryAsync(scope),
                Pipeline = await _repo.GetPipelineAsync(scope),
                LostReasons = await _repo.GetLostReasonsAsync(scope, null, null),
                MyToday = callList.Where(c => c.DueAt < DateTime.Today.AddDays(1)).Take(10).ToList(),
                UpcomingDemos = (await _repo.GetDemosAsync(scope, DateTime.Now, DateTime.Now.AddDays(14)))
                    .Where(d => d.Status is "Scheduled" or "Rescheduled").Take(8).ToList()
            });
        }

        // ── Call List ──
        public async Task<IActionResult> CallList(int? user, string? tab)
        {
            var scope = await GetScopeAsync();
            var team = await GetAssignableUsersAsync(scope);
            IEnumerable<int> ids = new[] { scope.UserId };
            if (user.HasValue && (scope.All || scope.UserIds.Contains(user.Value))) ids = new[] { user.Value };
            var items = await _repo.GetCallListAsync(ids);
            var now = DateTime.Now;
            var tomorrow = DateTime.Today.AddDays(1);
            var vm = new EnquiryCallListViewModel
            {
                Overdue = items.Where(i => i.DueAt < now).ToList(),
                Today = items.Where(i => i.DueAt >= now && i.DueAt < tomorrow).ToList(),
                Upcoming = items.Where(i => i.DueAt >= tomorrow).ToList(),
                NewOnes = items.Where(i => i.StageCode == EnquiryStageCodes.New && i.Attempts == 0).ToList(),
                Outcomes = await _repo.GetOutcomesAsync(true),
                TeamUsers = scope.CanManageTeam ? team : new List<UserMaster>(),
                UserFilter = user
            };
            vm.Tab = tab is "overdue" or "today" or "upcoming" or "new" ? tab : (vm.Overdue.Count > 0 ? "overdue" : "today");
            return View(vm);
        }

        // ── List and board ──
        public async Task<IActionResult> Index([FromQuery] EnquiryFilter filter, string? view)
        {
            var scope = await GetScopeAsync();
            return View(new EnquiryIndexViewModel
            {
                Enquiries = await _repo.GetEnquiriesAsync(scope, filter),
                Filter = filter,
                Options = await BuildOptionsAsync(scope),
                ViewMode = view == "board" ? "board" : "table",
                CanManageTeam = scope.CanManageTeam
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? clientId)
        {
            var scope = await GetScopeAsync();
            return View("Form", new EnquiryFormViewModel
            {
                Enquiry = new Enquiry { EnquiryClientId = clientId ?? 0, OwnerUserId = scope.UserId },
                FirstFollowUpAt = DateTime.Now.AddDays(1).Date.AddHours(11),
                Options = await BuildOptionsAsync(scope),
                CanChooseOwner = scope.CanManageTeam
            });
        }

        private async Task<string?> ValidateFormAsync(EnquiryFormViewModel m, EnquiryScope scope)
        {
            if (string.IsNullOrWhiteSpace(m.Enquiry.Title)) return "Enter a title for the enquiry.";
            if (m.Enquiry.EnquiryClientId == 0 || await _repo.GetClientAsync(m.Enquiry.EnquiryClientId, scope) == null) return "Select a client.";
            if (m.ProductIds.Count == 0) return "Select at least one product.";
            return null;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnquiryFormViewModel m)
        {
            var scope = await GetScopeAsync();
            var error = await ValidateFormAsync(m, scope);
            if (error == null && (m.FirstFollowUpAt == null || m.FirstFollowUpAt < DateTime.Now.AddMinutes(-5)))
                error = "Set a first follow-up date and time that isn't in the past.";
            if (error != null)
            {
                TempData["Error"] = error;
                m.Options = await BuildOptionsAsync(scope);
                m.CanChooseOwner = scope.CanManageTeam;
                return View("Form", m);
            }

            var owner = m.Enquiry.OwnerUserId;
            if (owner == 0 || !scope.CanManageTeam || (!scope.All && !scope.UserIds.Contains(owner))) owner = scope.UserId;
            m.Enquiry.OwnerUserId = owner;
            m.Enquiry.Title = m.Enquiry.Title.Trim();
            var id = await _repo.CreateEnquiryAsync(m.Enquiry, m.ProductIds, m.CollaboratorIds, new EnquiryFollowUp
            {
                FollowUpType = NormalizeType(m.FirstFollowUpType),
                DueAt = m.FirstFollowUpAt!.Value,
                Purpose = m.FirstFollowUpPurpose
            }, scope.UserId);

            if (owner != scope.UserId)
                _notifier.QueueUserMessage(owner, "New enquiry assigned to you", m.Enquiry.Title, $"/ClientEnquiry/Details/{id}");
            TempData["Success"] = "Enquiry created.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            return View("Form", new EnquiryFormViewModel
            {
                Enquiry = e,
                ProductIds = await _repo.GetEnquiryProductIdsAsync(id),
                CollaboratorIds = await _repo.GetCollaboratorIdsAsync(id),
                Options = await BuildOptionsAsync(scope)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnquiryFormViewModel m)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            m.Enquiry.Id = id;
            var error = await ValidateFormAsync(m, scope);
            if (error != null)
            {
                TempData["Error"] = error;
                m.Options = await BuildOptionsAsync(scope);
                return View("Form", m);
            }
            m.Enquiry.Title = m.Enquiry.Title.Trim();
            await _repo.UpdateEnquiryAsync(m.Enquiry, m.ProductIds, m.CollaboratorIds, scope.UserId);
            TempData["Success"] = "Enquiry updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Enquiry 360 ──
        public async Task<IActionResult> Details(int id)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            var client = await _repo.GetClientAsync(e.EnquiryClientId, new EnquiryScope { All = true, UserId = scope.UserId });
            return View(new EnquiryDetailsViewModel
            {
                Enquiry = e,
                Client = client,
                Activities = await _repo.GetActivitiesAsync(id),
                FollowUps = await _repo.GetFollowUpsAsync(id),
                Calls = await _repo.GetCallsAsync(id),
                Demos = await _repo.GetEnquiryDemosAsync(id),
                Attachments = await _repo.GetAttachmentsAsync(id),
                Stages = await _repo.GetStagesAsync(true),
                Outcomes = await _repo.GetOutcomesAsync(true),
                LostReasons = await _repo.GetLookupsAsync("EnquiryLostReason", true),
                Users = scope.CanManageTeam ? await GetAssignableUsersAsync(scope) : new List<UserMaster>(),
                Quotations = await _repo.GetQuotationOptionsAsync(),
                ClientCodes = (await _clientDetailsRepo.GetClientCodesWithDetailsAsync()).ToList(),
                CanManageTeam = scope.CanManageTeam
            });
        }

        // ── Stage ──
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> ChangeStage(int id, int stageId, DateTime? nextFollowUpAt, int? lostReasonId, string? lostRemarks, string? linkedClientCode)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            var stage = (await _repo.GetStagesAsync(true)).FirstOrDefault(s => s.Id == stageId);
            if (stage == null) { TempData["Error"] = "Select a valid stage."; return RedirectToAction(nameof(Details), new { id }); }

            string? error = null;
            if (stage.StageCode == EnquiryStageCodes.Lost && lostReasonId == null)
                error = "Select a reason for losing this enquiry.";
            else if (!stage.IsClosed && nextFollowUpAt == null && await _repo.OpenFollowUpCountAsync(id) == 0)
                error = "Set the next follow-up date, because an open enquiry must always have one.";
            else if (nextFollowUpAt < DateTime.Now.AddMinutes(-5))
                error = "The next follow-up can't be in the past.";
            if (error != null) { TempData["Error"] = error; return RedirectToAction(nameof(Details), new { id }); }

            await _repo.ChangeStageAsync(id, stage, scope.UserId, stage.IsClosed ? null : nextFollowUpAt, lostReasonId, lostRemarks, linkedClientCode);
            TempData["Success"] = $"Stage changed to {stage.Name}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Board drag-and-drop. Moves needing extra input (closing, reopening) go through the details page.
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> MoveStage([FromBody] EnquiryMoveRequest r)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(r.EnquiryId, scope);
            if (e == null) return Json(new { success = false, message = "Enquiry not found." });
            var stage = (await _repo.GetStagesAsync(true)).FirstOrDefault(s => s.Id == r.StageId);
            if (stage == null) return Json(new { success = false, message = "Stage not found." });
            if (stage.IsClosed || e.StageIsClosed)
                return Json(new { success = false, redirect = Url.Action(nameof(Details), new { id = e.Id }) + "?stage=" + stage.Id });
            await _repo.ChangeStageAsync(e.Id, stage, scope.UserId, null, null, null, null);
            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Reassign(int id, int newOwnerId, string? reason)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            var users = await GetAssignableUsersAsync(scope);
            var target = users.FirstOrDefault(u => u.Id == newOwnerId);
            if (!scope.CanManageTeam || target == null) { TempData["Error"] = "You can reassign only to users in your team."; return RedirectToAction(nameof(Details), new { id }); }
            if (string.IsNullOrWhiteSpace(reason)) { TempData["Error"] = "Enter a reason for reassigning."; return RedirectToAction(nameof(Details), new { id }); }
            await _repo.ReassignAsync(id, newOwnerId, target.FullName ?? target.Username, reason.Trim(), scope.UserId);
            _notifier.QueueUserMessage(newOwnerId, "Enquiry reassigned to you", $"{e.CompanyName} ({e.EnquiryNo})\n{reason.Trim()}", $"/ClientEnquiry/Details/{id}");
            TempData["Success"] = "Enquiry reassigned.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> BulkReassign(List<int> enquiryIds, int newOwnerId, string? reason)
        {
            var scope = await GetScopeAsync();
            var target = (await GetAssignableUsersAsync(scope)).FirstOrDefault(u => u.Id == newOwnerId);
            if (!scope.CanManageTeam || target == null || enquiryIds.Count == 0 || string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Select enquiries, a user in your team and a reason.";
                return RedirectToAction(nameof(Index));
            }
            var moved = 0;
            foreach (var id in enquiryIds.Distinct())
                if (await _repo.GetEnquiryAsync(id, scope) != null)
                {
                    await _repo.ReassignAsync(id, newOwnerId, target.FullName ?? target.Username, reason.Trim(), scope.UserId);
                    moved++;
                }
            if (moved > 0) _notifier.QueueUserMessage(newOwnerId, "Enquiries reassigned to you", $"{moved} enquiries were assigned to you.\n{reason.Trim()}", "/ClientEnquiry/CallList");
            TempData["Success"] = $"{moved} enquiries reassigned.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Hold(int id, DateTime? resumeDate, string? reason)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            if (resumeDate == null || resumeDate <= DateTime.Today) { TempData["Error"] = "Choose a resume date in the future."; return RedirectToAction(nameof(Details), new { id }); }
            await _repo.SetHoldAsync(id, resumeDate, reason, scope.UserId);
            TempData["Success"] = $"On hold until {resumeDate:dd MMM yyyy}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Resume(int id)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            await _repo.SetHoldAsync(id, null, null, scope.UserId);
            TempData["Success"] = "Enquiry resumed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            await _repo.SoftDeleteAsync(id, scope.UserId);
            TempData["Success"] = "Enquiry deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> AddNote(int id, string? note)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            if (string.IsNullOrWhiteSpace(note)) { TempData["Error"] = "Write a note first."; return RedirectToAction(nameof(Details), new { id }); }
            await _repo.AddNoteAsync(id, note.Trim(), scope.UserId);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> LinkQuotation(int id, int quotationId)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            var q = (await _repo.GetQuotationOptionsAsync()).FirstOrDefault(x => x.Id == quotationId);
            if (q == null) { TempData["Error"] = "Select a quotation."; return RedirectToAction(nameof(Details), new { id }); }
            await _repo.LinkQuotationAsync(id, q.Id, q.QuotationNo, scope.UserId);
            TempData["Success"] = $"Quotation {q.QuotationNo} linked.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Follow-ups ──
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> AddFollowUp(int id, string? type, DateTime? dueAt, string? purpose)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            if (e.StageIsClosed) { TempData["Error"] = "Reopen this enquiry before planning a follow-up."; return RedirectToAction(nameof(Details), new { id }); }
            if (dueAt == null || dueAt < DateTime.Now.AddMinutes(-5)) { TempData["Error"] = "Choose a follow-up time that isn't in the past."; return RedirectToAction(nameof(Details), new { id }); }
            await _repo.AddFollowUpAsync(new EnquiryFollowUp { EnquiryId = id, FollowUpType = NormalizeType(type), DueAt = dueAt.Value, AssignedToUserId = e.OwnerUserId, Purpose = purpose }, scope.UserId);
            TempData["Success"] = "Follow-up planned.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Next follow-up is required unless another one is open, so the enquiry always has a next action.
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> CompleteFollowUp(int followUpId, string? result, DateTime? nextAt, string? nextType, string? returnTo)
        {
            var scope = await GetScopeAsync();
            var f = await _repo.GetFollowUpAsync(followUpId);
            var e = f == null ? null : await _repo.GetEnquiryAsync(f.EnquiryId, scope);
            if (f == null || e == null) return NotFound();
            IActionResult Back() => returnTo == "calllist" ? RedirectToAction(nameof(CallList)) : RedirectToAction(nameof(Details), new { id = e.Id });
            if (f.Status != "Open") { TempData["Error"] = "This follow-up is already closed."; return Back(); }
            if (string.IsNullOrWhiteSpace(result)) { TempData["Error"] = "Enter the result of the follow-up."; return Back(); }
            var others = await _repo.OpenFollowUpCountAsync(e.Id) - 1;
            if (nextAt == null && others == 0) { TempData["Error"] = "Set the next follow-up date, or close the enquiry as Won or Lost."; return Back(); }
            if (nextAt < DateTime.Now.AddMinutes(-5)) { TempData["Error"] = "The next follow-up can't be in the past."; return Back(); }
            var next = nextAt == null ? null : new EnquiryFollowUp { FollowUpType = NormalizeType(nextType), DueAt = nextAt.Value, AssignedToUserId = e.OwnerUserId };
            await _repo.CompleteFollowUpAsync(followUpId, result.Trim(), next, scope.UserId);
            TempData["Success"] = "Follow-up completed.";
            return Back();
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> RescheduleFollowUp(int followUpId, DateTime? newDue, string? reason, string? returnTo)
        {
            var scope = await GetScopeAsync();
            var f = await _repo.GetFollowUpAsync(followUpId);
            var e = f == null ? null : await _repo.GetEnquiryAsync(f.EnquiryId, scope);
            if (f == null || e == null) return NotFound();
            IActionResult Back() => returnTo == "calllist" ? RedirectToAction(nameof(CallList)) : RedirectToAction(nameof(Details), new { id = e.Id });
            if (f.Status != "Open" || newDue == null || newDue < DateTime.Now.AddMinutes(-5) || string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Enter a future date and a reason to reschedule.";
                return Back();
            }
            await _repo.RescheduleFollowUpAsync(followUpId, newDue.Value, reason.Trim(), scope.UserId);
            TempData["Success"] = "Follow-up rescheduled.";
            return Back();
        }

        // ── Calls ──
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> LogCall(int id, int? followUpId, int? contactId, string? direction, string? phoneNumber, int outcomeId, int? durationMinutes, string? notes, DateTime? nextAt, string? nextType, string? returnTo)
        {
            var scope = await GetScopeAsync();
            var e = await _repo.GetEnquiryAsync(id, scope);
            if (e == null) return NotFound();
            IActionResult Back() => returnTo == "calllist" ? RedirectToAction(nameof(CallList)) : RedirectToAction(nameof(Details), new { id });
            var outcome = (await _repo.GetOutcomesAsync(true)).FirstOrDefault(o => o.Id == outcomeId);
            if (outcome == null) { TempData["Error"] = "Select the call outcome."; return Back(); }

            EnquiryFollowUp? follow = null;
            if (followUpId.HasValue)
            {
                follow = await _repo.GetFollowUpAsync(followUpId.Value);
                if (follow == null || follow.EnquiryId != id) { TempData["Error"] = "Follow-up not found."; return Back(); }
            }
            var open = await _repo.OpenFollowUpCountAsync(id);
            var remaining = follow?.Status == "Open" ? open - 1 : open;
            if (!e.StageIsClosed && nextAt == null && remaining == 0) { TempData["Error"] = "Set the next follow-up date, or close the enquiry as Won or Lost."; return Back(); }
            if (nextAt < DateTime.Now.AddMinutes(-5)) { TempData["Error"] = "The next follow-up can't be in the past."; return Back(); }

            await _repo.LogCallAsync(new EnquiryCallLog
            {
                EnquiryId = id,
                ContactId = contactId,
                FollowUpId = followUpId,
                Direction = direction == "Incoming" ? "Incoming" : "Outgoing",
                PhoneNumber = phoneNumber,
                StartedAt = DateTime.Now,
                DurationSeconds = Math.Max(durationMinutes ?? 0, 0) * 60,
                OutcomeId = outcomeId,
                Notes = notes?.Trim()
            }, outcome.IsConnected, nextAt == null || e.StageIsClosed ? null : new EnquiryFollowUp
            {
                FollowUpType = NormalizeType(nextType),
                DueAt = nextAt.Value,
                AssignedToUserId = e.OwnerUserId,
                Purpose = $"After call: {outcome.Name}"
            }, scope.UserId);
            TempData["Success"] = "Call logged.";
            return Back();
        }

        // ── Attachments ──
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile? file)
        {
            var scope = await GetScopeAsync();
            if (await _repo.GetEnquiryAsync(id, scope) == null) return NotFound();
            if (file == null || file.Length == 0) { TempData["Error"] = "Choose a file."; return RedirectToAction(nameof(Details), new { id }); }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext) || file.Length > MaxFileBytes)
            {
                TempData["Error"] = "Upload a PDF, image, Word or Excel file of up to 10 MB.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var folder = AttachmentFolder();
            Directory.CreateDirectory(folder);
            var stored = Guid.NewGuid().ToString("N") + ext;
            await using (var stream = System.IO.File.Create(Path.Combine(folder, stored)))
                await file.CopyToAsync(stream);
            await _repo.AddAttachmentAsync(new EnquiryAttachment { EnquiryId = id, FileName = Path.GetFileName(file.FileName), StoredName = stored }, scope.UserId);
            TempData["Success"] = "File attached.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Kept outside wwwroot so files are served only through this visibility check.
        private string AttachmentFolder() => Path.Combine(_env.ContentRootPath, "EnquiryAttachments");

        public async Task<IActionResult> Attachment(int attachmentId)
        {
            var scope = await GetScopeAsync();
            var a = await _repo.GetAttachmentAsync(attachmentId);
            if (a == null || await _repo.GetEnquiryAsync(a.EnquiryId, scope) == null) return NotFound();
            var path = Path.Combine(AttachmentFolder(), Path.GetFileName(a.StoredName));
            if (!System.IO.File.Exists(path)) return NotFound();
            return PhysicalFile(path, "application/octet-stream", a.FileName);
        }

        // ── Reports and export ──
        public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
        {
            var scope = await GetScopeAsync();
            var f = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var t = to ?? DateTime.Today;
            return View(new EnquiryReportsViewModel
            {
                From = f,
                To = t,
                Funnel = await _repo.GetPipelineAsync(scope),
                Sources = await _repo.GetBySourceAsync(scope, f, t),
                LostReasons = await _repo.GetLostReasonsAsync(scope, f, t),
                Calls = await _repo.GetCallOutcomesAsync(scope, f, t),
                Demos = await _repo.GetDemoOutcomesAsync(scope, f, t),
                Users = await _repo.GetUserActivityAsync(scope, f, t)
            });
        }

        public async Task<IActionResult> ExportExcel([FromQuery] EnquiryFilter filter)
        {
            var scope = await GetScopeAsync();
            var rows = await _repo.GetEnquiriesAsync(scope, filter);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Enquiries");
            var headers = new[] { "Enquiry No", "Title", "Company", "Contact", "Mobile", "Products", "Source", "Stage", "Priority", "Owner", "Expected Value", "Next Follow-up", "Created" };
            for (var i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
            ws.Row(1).Style.Font.Bold = true;
            var r = 2;
            foreach (var e in rows)
            {
                ws.Cell(r, 1).Value = e.EnquiryNo;
                ws.Cell(r, 2).Value = e.Title;
                ws.Cell(r, 3).Value = e.CompanyName;
                ws.Cell(r, 4).Value = e.PrimaryContactName;
                ws.Cell(r, 5).Value = e.PrimaryMobile;
                ws.Cell(r, 6).Value = e.ProductNames;
                ws.Cell(r, 7).Value = e.SourceName;
                ws.Cell(r, 8).Value = e.StageName;
                ws.Cell(r, 9).Value = e.PriorityName;
                ws.Cell(r, 10).Value = e.OwnerName;
                ws.Cell(r, 11).Value = e.ExpectedValue;
                ws.Cell(r, 12).Value = e.NextFollowUpAt?.ToString("dd MMM yyyy hh:mm tt");
                ws.Cell(r, 13).Value = e.CreatedAt.ToString("dd MMM yyyy");
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Enquiries_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
