using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class HelpDeskTicketController : Controller
    {
        private readonly IHelpDeskTicketRepository _ticketRepo;
        private readonly ITicketCategoryRepository _categoryRepo;
        private readonly ITicketSubCategoryRepository _subCategoryRepo;
        private readonly ITicketPriorityRepository _priorityRepo;
        private readonly ITicketEmailService _ticketEmailService;
        private readonly ITicketBrowserNotificationService _ticketNotificationService;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;
    private readonly IFinancialYearMasterRepository _fyRepo;

        public HelpDeskTicketController(
            IHelpDeskTicketRepository ticketRepo,
            ITicketCategoryRepository categoryRepo,
            ITicketSubCategoryRepository subCategoryRepo,
            ITicketPriorityRepository priorityRepo,
            ITicketEmailService ticketEmailService,
            ITicketBrowserNotificationService ticketNotificationService,
            IUserRepository userRepo,
IWebHostEnvironment env,
        IFinancialYearMasterRepository fyRepo)
    {
        _ticketRepo = ticketRepo;
        _categoryRepo = categoryRepo;
        _subCategoryRepo = subCategoryRepo;
        _priorityRepo = priorityRepo;
        _ticketEmailService = ticketEmailService;
        _ticketNotificationService = ticketNotificationService;
        _userRepo = userRepo;
        _env = env;
        _fyRepo = fyRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string CurrentRole => User.FindFirstValue(ClaimTypes.Role) ?? "";
        private bool IsAgentOrAdmin => User.IsInRole("Administrator") || User.IsInRole("Ticket Admin") || User.IsInRole("Ticket Agent");

        // ── Ticket List (All tickets for agents/admins) ──
        [Authorize(Roles = "Administrator,Ticket Admin,Ticket Agent")]
        public async Task<IActionResult> Index(string? status, string? priority, int? category, int? assignedTo, DateTime? from, DateTime? to)
        {
            // Ticket Agents see tickets currently or previously assigned to them
            var allTickets = User.IsInRole("Ticket Agent") && !User.IsInRole("Ticket Admin") && !User.IsInRole("Administrator")
                ? (await _ticketRepo.GetTicketsForAgentAsync(CurrentUserId)).ToList()
                : (await _ticketRepo.GetAllAsync()).ToList();

            var fromDate = from?.Date;
            var toDate = to?.Date;
            var filtered = allTickets.AsEnumerable();

            if (!string.IsNullOrEmpty(status))
                filtered = filtered.Where(t => t.Status == status);
            if (!string.IsNullOrEmpty(priority))
                filtered = filtered.Where(t => t.PriorityName == priority);
            if (category.HasValue)
                filtered = filtered.Where(t => t.CategoryId == category.Value);
            if (assignedTo.HasValue)
                filtered = filtered.Where(t => t.AssignedToId == assignedTo.Value);
            if (fromDate.HasValue)
                filtered = filtered.Where(t => t.CreatedAt.Date >= fromDate.Value);
            if (toDate.HasValue)
                filtered = filtered.Where(t => t.CreatedAt.Date <= toDate.Value);

            var tickets = filtered.ToList();

            var vm = new TicketListViewModel
            {
                Tickets = tickets,
                StatusFilter = status,
                PriorityFilter = priority,
                CategoryFilter = category,
                AssignedToFilter = assignedTo,
                FromDate = fromDate?.ToString("yyyy-MM-dd"),
                ToDate = toDate?.ToString("yyyy-MM-dd"),
                TotalCount = allTickets.Count,
                OpenCount = allTickets.Count(t => t.Status == "Open"),
                InProgressCount = allTickets.Count(t => t.Status == "In Progress"),
                WaitingCount = allTickets.Count(t => t.Status == "Waiting for Client"),
                ResolvedCount = allTickets.Count(t => t.Status == "Resolved"),
                ClosedCount = allTickets.Count(t => t.Status == "Closed")
            };

            ViewBag.Categories = (await _categoryRepo.GetAllActiveAsync()).ToList();
            ViewBag.Agents = (await _ticketRepo.GetAgentsAsync()).ToList();
            return View(vm);
        }

        // ── My Tickets (for the ticket creator / client) ──
        public async Task<IActionResult> MyTickets(string? status, DateTime? from, DateTime? to)
        {
            var userId = CurrentUserId;
            var allTickets = (await _ticketRepo.GetByCreatorAsync(userId)).ToList();
            var filtered = allTickets.AsEnumerable();

            if (!string.IsNullOrEmpty(status))
                filtered = filtered.Where(t => t.Status == status);

            var fromDate = from?.Date;
            var toDate = to?.Date;
            if (fromDate.HasValue)
                filtered = filtered.Where(t => t.CreatedAt.Date >= fromDate.Value);
            if (toDate.HasValue)
                filtered = filtered.Where(t => t.CreatedAt.Date <= toDate.Value);

            var tickets = filtered.ToList();

            var vm = new TicketListViewModel
            {
                Tickets = tickets,
                StatusFilter = status,
                FromDate = fromDate?.ToString("yyyy-MM-dd"),
                ToDate = toDate?.ToString("yyyy-MM-dd"),
                TotalCount = allTickets.Count,
                OpenCount = allTickets.Count(t => t.Status == "Open"),
                InProgressCount = allTickets.Count(t => t.Status == "In Progress"),
                WaitingCount = allTickets.Count(t => t.Status == "Waiting for Client"),
                ResolvedCount = allTickets.Count(t => t.Status == "Resolved"),
                ClosedCount = allTickets.Count(t => t.Status == "Closed")
            };

            return View(vm);
        }

        // ── Create ticket ──
        public async Task<IActionResult> Create()
        {
            var vm = new CreateTicketViewModel
            {
                Categories = (await _categoryRepo.GetAllActiveAsync()).ToList(),
                SubCategories = (await _subCategoryRepo.GetAllActiveAsync()).ToList(),
                Priorities = (await _priorityRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = (await _categoryRepo.GetAllActiveAsync()).ToList();
                vm.SubCategories = (await _subCategoryRepo.GetAllActiveAsync()).ToList();
                vm.Priorities = (await _priorityRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var userId = CurrentUserId;
            var ticketNumber = await _ticketRepo.GenerateTicketNumberAsync();

            var ticket = new HelpDeskTicket
            {
                TicketNumber = ticketNumber,
                Subject = vm.Subject.Trim(),
                Description = vm.Description.Trim(),
                CategoryId = vm.CategoryId,
                SubCategoryId = vm.SubCategoryId,
                PriorityId = vm.PriorityId,
                Status = "Open",
                CreatedById = userId,
                FinancialYearId = await _fyRepo.GetCurrentFYIdAsync()
            };

            var ticketId = await _ticketRepo.CreateAsync(ticket);

            // Add audit log
            await _ticketRepo.AddAuditLogAsync(new TicketAuditLog
            {
                TicketId = ticketId,
                Action = "Ticket Created",
                NewValue = ticketNumber,
                PerformedById = userId
            });

            // Handle attachments
            if (vm.Attachments != null)
            {
                foreach (var file in vm.Attachments)
                {
                    if (file.Length > 0)
                        await SaveAttachmentAsync(file, ticketId, null, userId);
                }
            }

            // Send email notification to Ticket Admins
            var createdTicket = await _ticketRepo.GetByIdAsync(ticketId);
            if (createdTicket != null)
            {
                _ = _ticketEmailService.NotifyTicketCreatedAsync(createdTicket);
                _ = _ticketNotificationService.NotifyTicketCreatedAsync(createdTicket);
            }

            TempData["Success"] = $"Ticket <strong>{ticketNumber}</strong> created successfully.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // ── Ticket Details / Conversation ──
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            var userId = CurrentUserId;
            var isAdmin = User.IsInRole("Administrator");
            var isTicketAdmin = User.IsInRole("Ticket Admin");
            var isTicketAgent = User.IsInRole("Ticket Agent");

            // Clients can only view their own tickets
            if (!IsAgentOrAdmin && ticket.CreatedById != userId)
                return Forbid();

            // Determine CanAct:
            // - Administrator: always can act
            // - Ticket Admin: can act on unassigned tickets OR tickets assigned to them
            // - Ticket Agent: can act only if ticket is currently assigned to them
            // Rule: whoever the ticket is last assigned to gets full access
            bool canAct;
            if (isAdmin)
                canAct = true;
            else if (isTicketAdmin && !isAdmin)
                canAct = ticket.AssignedToId == null || ticket.AssignedToId == userId;
            else if (isTicketAgent)
                canAct = ticket.AssignedToId == userId;
            else
                canAct = false; // clients — handled separately in the view

            var messages = (await _ticketRepo.GetMessagesAsync(id)).ToList();
            var attachments = (await _ticketRepo.GetAttachmentsAsync(id)).ToList();
            var auditLogs = (await _ticketRepo.GetAuditLogsAsync(id)).ToList();
            var agents = (await _ticketRepo.GetAgentsAsync()).ToList();

            var vm = new TicketDetailViewModel
            {
                Ticket = ticket,
                Messages = messages,
                Attachments = attachments,
                AuditLogs = auditLogs,
                Agents = agents,
                IsAgent = IsAgentOrAdmin,
                IsAdmin = isAdmin || isTicketAdmin,
                CanAct = canAct
            };

            return View(vm);
        }

        // ── Reply to ticket ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int ticketId, string replyMessage, bool isInternal, bool markAsClosed, List<IFormFile>? replyAttachments)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return NotFound();

            var userId = CurrentUserId;
            if (!IsAgentOrAdmin && ticket.CreatedById != userId)
                return Forbid();

            // Server-side CanAct check for agents/admins
            if (IsAgentOrAdmin && !User.IsInRole("Administrator"))
            {
                var isTicketAdmin = User.IsInRole("Ticket Admin");
                var isTicketAgent = User.IsInRole("Ticket Agent");

                bool canAct = isTicketAdmin ? (ticket.AssignedToId == null || ticket.AssignedToId == userId)
                            : isTicketAgent ? ticket.AssignedToId == userId
                            : false;

                if (!canAct)
                {
                    TempData["Error"] = "You can only view this ticket. Reply is not allowed.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }
            }

            // If client wants to close, reply message is mandatory
            if (markAsClosed && !IsAgentOrAdmin && string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["Error"] = "A closing comment is required when marking the ticket as closed.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            if (string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var message = new TicketMessage
            {
                TicketId = ticketId,
                SenderId = userId,
                Message = replyMessage.Trim(),
                IsInternal = isInternal && IsAgentOrAdmin // Only agents can post internal notes
            };

            var messageId = await _ticketRepo.AddMessageAsync(message);

            // Set first response time if this is agent's first reply
            if (IsAgentOrAdmin && ticket.CreatedById != userId)
                await _ticketRepo.SetFirstResponseAsync(ticketId);

            // Handle attachments
            if (replyAttachments != null)
            {
                foreach (var file in replyAttachments)
                {
                    if (file.Length > 0)
                        await SaveAttachmentAsync(file, ticketId, messageId, userId);
                }
            }

            // Send email notification for reply
            var senderName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "Unknown";
            _ = _ticketEmailService.NotifyNewReplyAsync(ticket, senderName, replyMessage.Trim(), message.IsInternal);
            _ = _ticketNotificationService.NotifyNewReplyAsync(ticket, senderName, replyMessage.Trim(), message.IsInternal);

            // Client marked ticket as closed
            if (markAsClosed && !IsAgentOrAdmin && ticket.CreatedById == userId)
            {
                var oldStatus = ticket.Status;
                await _ticketRepo.UpdateStatusAsync(ticketId, "Closed", null, DateTime.Now);

                await _ticketRepo.AddAuditLogAsync(new TicketAuditLog
                {
                    TicketId = ticketId,
                    Action = "Status Changed",
                    OldValue = oldStatus,
                    NewValue = "Closed",
                    PerformedById = userId
                });

                _ = _ticketEmailService.NotifyStatusChangedAsync(ticket, oldStatus, "Closed");
                _ = _ticketNotificationService.NotifyStatusChangedAsync(ticket, oldStatus, "Closed");

                TempData["Success"] = "Reply posted and ticket has been <strong>closed</strong>.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            TempData["Success"] = "Reply posted.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // ── Update ticket status ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ticketId, string newStatus, string? statusNote)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return NotFound();

            var userId = CurrentUserId;

            // Server-side CanAct check for agents/admins
            if (IsAgentOrAdmin && !User.IsInRole("Administrator"))
            {
                var isTicketAdmin = User.IsInRole("Ticket Admin");
                var isTicketAgent = User.IsInRole("Ticket Agent");

                bool canAct = isTicketAdmin ? (ticket.AssignedToId == null || ticket.AssignedToId == userId)
                            : isTicketAgent ? ticket.AssignedToId == userId
                            : false;

                if (!canAct)
                {
                    TempData["Error"] = "You can only view this ticket. Status change is not allowed.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }
            }

            var validStatuses = new[] { "Open", "In Progress", "Waiting for Client", "Resolved", "Closed" };
            if (!validStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            // Mandatory note for Resolved / Closed
            if (newStatus is "Resolved" or "Closed" && string.IsNullOrWhiteSpace(statusNote))
            {
                TempData["Error"] = "A note is required when changing status to Resolved or Closed.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            // Clients can only close a resolved ticket
            if (!IsAgentOrAdmin)
            {
                if (ticket.CreatedById != userId || newStatus != "Closed" || ticket.Status != "Resolved")
                {
                    TempData["Error"] = "You can only close a resolved ticket.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }
            }

            var oldStatus = ticket.Status;
            DateTime? resolvedAt = newStatus == "Resolved" ? DateTime.Now : null;
            DateTime? closedAt = newStatus == "Closed" ? DateTime.Now : null;

            await _ticketRepo.UpdateStatusAsync(ticketId, newStatus, resolvedAt, closedAt);

            // Save the mandatory note as a conversation message
            if (!string.IsNullOrWhiteSpace(statusNote))
            {
                await _ticketRepo.AddMessageAsync(new TicketMessage
                {
                    TicketId = ticketId,
                    SenderId = userId,
                    Message = statusNote.Trim(),
                    IsInternal = false
                });
            }

            await _ticketRepo.AddAuditLogAsync(new TicketAuditLog
            {
                TicketId = ticketId,
                Action = "Status Changed",
                OldValue = oldStatus,
                NewValue = newStatus,
                PerformedById = userId
            });

            // Send email notification for status change
            _ = _ticketEmailService.NotifyStatusChangedAsync(ticket, oldStatus, newStatus);
            _ = _ticketNotificationService.NotifyStatusChangedAsync(ticket, oldStatus, newStatus);

            TempData["Success"] = $"Ticket status changed to <strong>{newStatus}</strong>.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // ── Assign ticket ──
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin,Ticket Agent")]
        public async Task<IActionResult> Assign(int ticketId, int agentId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return NotFound();

            var userId = CurrentUserId;

            // Server-side CanAct check for agents/admins
            if (!User.IsInRole("Administrator"))
            {
                var isTicketAdmin = User.IsInRole("Ticket Admin");
                var isTicketAgent = User.IsInRole("Ticket Agent");

                bool canAct = isTicketAdmin ? (ticket.AssignedToId == null || ticket.AssignedToId == userId)
                            : isTicketAgent ? ticket.AssignedToId == userId
                            : false;

                if (!canAct)
                {
                    TempData["Error"] = "You can only view this ticket. Assignment is not allowed.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }
            }

            var oldAssignee = ticket.AssignedToName ?? "Unassigned";

            await _ticketRepo.AssignAsync(ticketId, agentId);

            // Get agent name for audit
            var agents = await _ticketRepo.GetAgentsAsync();
            var agentName = agents.FirstOrDefault(a => a.Id == agentId)?.FullName ?? "Unknown";

            await _ticketRepo.AddAuditLogAsync(new TicketAuditLog
            {
                TicketId = ticketId,
                Action = "Ticket Assigned",
                OldValue = oldAssignee,
                NewValue = agentName,
                PerformedById = userId
            });

            // Send email notification for assignment
            var agentUser = await _userRepo.GetByIdAsync(agentId);
            if (agentUser != null && !string.IsNullOrWhiteSpace(agentUser.Email))
                _ = _ticketEmailService.NotifyTicketAssignedAsync(ticket, agentName, agentUser.Email);
            _ = _ticketNotificationService.NotifyTicketAssignedAsync(ticket, agentName, agentId);

            TempData["Success"] = $"Ticket assigned to <strong>{agentName}</strong>.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // ── Download attachment ──
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _ticketRepo.GetAttachmentByIdAsync(id);
            if (attachment == null) return NotFound();

            // Security: verify user has access to this ticket
            var ticket = await _ticketRepo.GetByIdAsync(attachment.TicketId);
            if (ticket == null) return NotFound();
            if (!IsAgentOrAdmin && ticket.CreatedById != CurrentUserId)
                return Forbid();

            var fullPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var contentType = GetContentType(attachment.FileName);
            return PhysicalFile(fullPath, contentType, attachment.FileName);
        }

        // ── Preview attachment (inline) ──
        public async Task<IActionResult> PreviewAttachment(int id)
        {
            var attachment = await _ticketRepo.GetAttachmentByIdAsync(id);
            if (attachment == null) return NotFound();

            var ticket = await _ticketRepo.GetByIdAsync(attachment.TicketId);
            if (ticket == null) return NotFound();
            if (!IsAgentOrAdmin && ticket.CreatedById != CurrentUserId)
                return Forbid();

            var fullPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var contentType = GetContentType(attachment.FileName);
            return PhysicalFile(fullPath, contentType);
        }

        // ── API: Get sub-categories by category (for cascading dropdown) ──
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subs = await _subCategoryRepo.GetByCategoryIdAsync(categoryId);
            return Json(subs.Select(s => new { s.Id, s.SubCategoryName }));
        }

        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".csv" => "text/csv",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream"
            };
        }

        // ── Private helpers ──

        private async Task SaveAttachmentAsync(IFormFile file, int ticketId, int? messageId, int userId)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "tickets", ticketId.ToString());
            Directory.CreateDirectory(uploadsDir);

            var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/tickets/{ticketId}/{safeFileName}";

            await _ticketRepo.AddAttachmentAsync(new TicketAttachment
            {
                TicketId = ticketId,
                MessageId = messageId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSize = file.Length,
                UploadedById = userId
            });
        }
    }
}
