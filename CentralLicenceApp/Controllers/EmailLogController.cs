using System;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmailLogController : Controller
    {
        private readonly IEmailLogRepository _repo;

        public EmailLogController(IEmailLogRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? emailType, int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _repo.GetPagedAsync(fromDate, toDate, emailType, page, pageSize);

            ViewData["Title"] = "Email Log";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.EmailType = emailType;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.AvailableEmailTypes = (await _repo.GetDistinctEmailTypesAsync()).ToList();

            return View(items.ToList());
        }
    }
}