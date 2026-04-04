using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Microsoft.AspNetCore.Hosting;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CentralLicenceApp.Services
{
    public class DocumentPdfService : IDocumentPdfService
    {
        private readonly IViewRenderService _viewRenderer;
        private readonly IWebHostEnvironment _env;
        private readonly IBrowserProvider   _browserProvider;

        private string PdfStoreRoot => Path.Combine(_env.ContentRootPath, "PdfStore");

        public DocumentPdfService(IViewRenderService viewRenderer, IWebHostEnvironment env, IBrowserProvider browserProvider)
        {
            _viewRenderer    = viewRenderer;
            _env             = env;
            _browserProvider = browserProvider;
        }

        public async Task<(byte[] Bytes, string SavedPath)> GenerateInvoicePdfAsync(Invoice invoice)
        {
            var html = await _viewRenderer.RenderToStringAsync("Invoice/Print", invoice);
            html = RemovePrintToolbar(html);
            html = InlineLocalImages(html);
            var pdfBytes = await RenderHtmlToPdfAsync(html);
            var dir = Path.Combine(PdfStoreRoot, "Invoices");
            Directory.CreateDirectory(dir);
            var fileName  = $"Invoice_{SanitizeFileName(invoice.InvoiceNo)}.pdf";
            var savedPath = Path.Combine(dir, fileName);
            await File.WriteAllBytesAsync(savedPath, pdfBytes);
            return (pdfBytes, savedPath);
        }

        public async Task<(byte[] Bytes, string SavedPath)> GenerateQuotationPdfAsync(Quotation quotation)
        {
            var html = await _viewRenderer.RenderToStringAsync("Quotation/Print", quotation);
            html = RemovePrintToolbar(html);
            html = InlineLocalImages(html);
            var pdfBytes = await RenderHtmlToPdfAsync(html);
            var dir = Path.Combine(PdfStoreRoot, "Quotations");
            Directory.CreateDirectory(dir);
            var fileName  = $"Quotation_{SanitizeFileName(quotation.QuotationNo)}.pdf";
            var savedPath = Path.Combine(dir, fileName);
            await File.WriteAllBytesAsync(savedPath, pdfBytes);
            return (pdfBytes, savedPath);
        }

        private async Task<byte[]> RenderHtmlToPdfAsync(string html)
        {
            // Reuse the shared browser — no cold-start on each call.
            var browser = await _browserProvider.GetBrowserAsync();
            await using var page = await browser.NewPageAsync();

            // All images are already base64-inlined, so WaitUntil.Load is sufficient
            // and much faster than Networkidle0.
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load },
                Timeout   = 15_000
            });

            await page.EmulateMediaTypeAsync(MediaType.Print);

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format              = PaperFormat.A4,
                PrintBackground     = true,
                DisplayHeaderFooter = false,
                MarginOptions       = new MarginOptions { Top = "0mm", Right = "0mm", Bottom = "0mm", Left = "0mm" }
            });

            return pdfBytes;
        }

        private static string RemovePrintToolbar(string html)
        {
            return Regex.Replace(
                html,
                @"<div[^>]+class=""no-print""[^>]*>[\s\S]*?</div>",
                string.Empty,
                RegexOptions.IgnoreCase);
        }

        private string InlineLocalImages(string html)
        {
            var wwwroot = _env.WebRootPath;
            return Regex.Replace(
                html,
                "src=\"(/[^\"?#]+)\"",
                match =>
                {
                    var relPath  = match.Groups[1].Value;
                    var fullPath = Path.Combine(wwwroot, relPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(fullPath)) return match.Value;
                    var ext      = Path.GetExtension(fullPath).TrimStart('.').ToLowerInvariant();
                    var mimeType = ext switch
                    {
                        "svg"             => "image/svg+xml",
                        "jpg" or "jpeg"   => "image/jpeg",
                        "gif"             => "image/gif",
                        "webp"            => "image/webp",
                        _                 => "image/png"
                    };
                    var base64 = Convert.ToBase64String(File.ReadAllBytes(fullPath));
                    return "src=\"data:" + mimeType + ";base64," + base64 + "\"";
                },
                RegexOptions.IgnoreCase);
        }

        private static string SanitizeFileName(string name) =>
            Regex.Replace(name, "[/\\\\:*?\"<>|]", "-");
    }
}
