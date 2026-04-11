using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin")]
    public class TicketPriorityController : Controller
    {
        private readonly ITicketPriorityRepository _repo;

        public TicketPriorityController(ITicketPriorityRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var priorities = await _repo.GetAllAsync();
            return View(priorities.ToList());
        }

        public IActionResult Create()
        {
            return View(new TicketPriorityFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketPriorityFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var priority = new TicketPriorityMaster
            {
                PriorityName = vm.PriorityName.Trim(),
                ColorCode = vm.ColorCode?.Trim(),
                SortOrder = vm.SortOrder,
                SlaResponseHours = vm.SlaResponseHours,
                SlaResolutionHours = vm.SlaResolutionHours,
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(priority);
            TempData["Success"] = $"Ticket priority <strong>{priority.PriorityName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var priority = await _repo.GetByIdAsync(id);
            if (priority == null) return NotFound();

            var vm = new TicketPriorityFormViewModel
            {
                Id = priority.Id,
                PriorityName = priority.PriorityName,
                ColorCode = priority.ColorCode,
                SortOrder = priority.SortOrder,
                SlaResponseHours = priority.SlaResponseHours,
                SlaResolutionHours = priority.SlaResolutionHours,
                IsActive = priority.IsActive
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketPriorityFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var priority = new TicketPriorityMaster
            {
                Id = vm.Id,
                PriorityName = vm.PriorityName.Trim(),
                ColorCode = vm.ColorCode?.Trim(),
                SortOrder = vm.SortOrder,
                SlaResponseHours = vm.SlaResponseHours,
                SlaResolutionHours = vm.SlaResolutionHours,
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(priority);
            TempData["Success"] = "Ticket priority updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var validation = await _repo.ValidateDeleteAsync(id);
            if (!validation.CanDelete)
            {
                TempData["Error"] = validation.Reason;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "The selected ticket priority was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This ticket priority cannot be deleted because related ticket records still reference it.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Ticket priority deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
