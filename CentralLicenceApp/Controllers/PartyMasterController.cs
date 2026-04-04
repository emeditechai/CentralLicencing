using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PartyMasterController : Controller
    {
        private readonly IPartyMasterRepository _repo;

        public PartyMasterController(IPartyMasterRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var parties = await _repo.GetAllAsync();
            return View(parties.ToList());
        }

        public IActionResult Create()
        {
            return View(new PartyMasterFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartyMasterFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var party = new PartyMaster
            {
                PartyName     = vm.PartyName.Trim(),
                ContactPerson = vm.ContactPerson?.Trim(),
                Mobile        = vm.Mobile?.Trim(),
                Email         = vm.Email?.Trim(),
                Address       = vm.Address?.Trim(),
                GSTINNo       = vm.GSTINNo?.Trim().ToUpperInvariant(),
                PANNo         = vm.PANNo?.Trim().ToUpperInvariant(),
                IsActive      = vm.IsActive
            };

            await _repo.CreateAsync(party);
            TempData["Success"] = $"Party <strong>{party.PartyName}</strong> created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var party = await _repo.GetByIdAsync(id);
            if (party == null) return NotFound();

            var vm = new PartyMasterFormViewModel
            {
                Id            = party.Id,
                PartyName     = party.PartyName,
                ContactPerson = party.ContactPerson,
                Mobile        = party.Mobile,
                Email         = party.Email,
                Address       = party.Address,
                GSTINNo       = party.GSTINNo,
                PANNo         = party.PANNo,
                IsActive      = party.IsActive
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartyMasterFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var party = new PartyMaster
            {
                Id            = vm.Id,
                PartyName     = vm.PartyName.Trim(),
                ContactPerson = vm.ContactPerson?.Trim(),
                Mobile        = vm.Mobile?.Trim(),
                Email         = vm.Email?.Trim(),
                Address       = vm.Address?.Trim(),
                GSTINNo       = vm.GSTINNo?.Trim().ToUpperInvariant(),
                PANNo         = vm.PANNo?.Trim().ToUpperInvariant(),
                IsActive      = vm.IsActive
            };

            await _repo.UpdateAsync(party);
            TempData["Success"] = "Party updated successfully.";
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

            var deleted = await _repo.DeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] = deleted
                ? "Party deleted."
                : "Party not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}
