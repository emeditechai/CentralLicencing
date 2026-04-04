using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Services
{
    public interface IDocumentPdfService
    {
        /// <summary>
        /// Renders the Invoice/Print view to PDF, saves it under PdfStore/Invoices/ and
        /// returns the raw bytes (for email attachment).
        /// </summary>
        Task<(byte[] Bytes, string SavedPath)> GenerateInvoicePdfAsync(Invoice invoice);

        /// <summary>
        /// Renders the Quotation/Print view to PDF, saves it under PdfStore/Quotations/ and
        /// returns the raw bytes (for email attachment).
        /// </summary>
        Task<(byte[] Bytes, string SavedPath)> GenerateQuotationPdfAsync(Quotation quotation);
    }
}
