using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BankMasterController : Controller
    {
        private readonly IBankMasterRepository _repo;

        public BankMasterController(IBankMasterRepository repo)
        {
            _repo = repo;
        }

        // GET /BankMaster
        public async Task<IActionResult> Index()
        {
            var banks = await _repo.GetAllAsync();
            return View(banks);
        }

        // GET /BankMaster/Create
        public IActionResult Create()
        {
            return View(new BankMaster { IsActive = true });
        }

        // POST /BankMaster/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankMaster model)
        {
            if (model.IsPrimary)
            {
                var existing = await _repo.GetPrimaryAsync();
                if (existing != null)
                    ModelState.AddModelError(nameof(BankMaster.IsPrimary),
                        $"'{existing.BankName}' is already set as primary. Please unset it first before marking another bank as primary.");
            }

            if (!ModelState.IsValid) return View(model);

            await _repo.CreateAsync(model);
            TempData["Success"] = $"Bank <strong>{model.BankName}</strong> added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /BankMaster/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bank = await _repo.GetByIdAsync(id);
            if (bank == null) return NotFound();
            return View(bank);
        }

        // POST /BankMaster/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BankMaster model)
        {
            if (id != model.Id) return BadRequest();

            if (model.IsPrimary)
            {
                var existing = await _repo.GetPrimaryAsync();
                if (existing != null && existing.Id != model.Id)
                    ModelState.AddModelError(nameof(BankMaster.IsPrimary),
                        $"'{existing.BankName}' is already set as primary. Please unset it first before marking another bank as primary.");
            }

            if (!ModelState.IsValid) return View(model);

            await _repo.UpdateAsync(model);
            TempData["Success"] = $"Bank <strong>{model.BankName}</strong> updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST /BankMaster/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bank = await _repo.GetByIdAsync(id);
            if (bank == null) return NotFound();

            await _repo.DeleteAsync(id);
            TempData["Success"] = $"Bank <strong>{bank.BankName}</strong> deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
