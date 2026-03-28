using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IClientLicenseRepository _clientLicenseRepository;
        private readonly IClientDetailsReportExportService _clientDetailsReportExportService;

        public ReportsController(
            IReportRepository reportRepository,
            IClientLicenseRepository clientLicenseRepository,
            IClientDetailsReportExportService clientDetailsReportExportService)
        {
            _reportRepository = reportRepository;
            _clientLicenseRepository = clientLicenseRepository;
            _clientDetailsReportExportService = clientDetailsReportExportService;
        }

        public async Task<IActionResult> ClientDetails(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            var items = ModelState.IsValid
                ? await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType)
                : Array.Empty<CentralLicenceApp.Models.Reports.ClientDetailsReportRow>();

            var vm = new ClientDetailsReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProductType = productType,
                ProductTypes = (await _clientLicenseRepository.GetDistinctProductTypesAsync()).ToList(),
                Items = items.ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> ExportClientDetailsExcel(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ClientDetails), new { fromDate, toDate, productType });
            }

            var items = await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType);
            var bytes = _clientDetailsReportExportService.GenerateExcel(items, productType, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ClientDetailsReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportClientDetailsPdf(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ClientDetails), new { fromDate, toDate, productType });
            }

            var items = await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType);
            var bytes = _clientDetailsReportExportService.GeneratePdf(items, productType, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"ClientDetailsReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        private static string? ValidateDateRange(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                return "From Date cannot be later than To Date.";
            }

            return null;
        }

        private static string? FormatDate(DateTime? value)
        {
            return value?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}