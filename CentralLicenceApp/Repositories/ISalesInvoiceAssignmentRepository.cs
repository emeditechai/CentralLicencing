using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ISalesInvoiceAssignmentRepository
    {
        Task<IEnumerable<SalesInvoiceAssignmentRow>> GetAssignmentsAsync(int? salesUserId, DateTime? from, DateTime? to);
        Task<IEnumerable<SalesInvoiceAssignmentRow>> GetUnassignedInvoicesAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<InvoiceLineItemDto>> GetInvoiceLineItemsAsync(int invoiceId);
        Task<bool> AssignWithLinesAsync(AssignInvoiceRequest request, int assignedById);
        Task<bool> UnassignAsync(int assignmentId);
        Task<IEnumerable<SalesInvoiceAssignmentLine>> GetAssignmentLinesAsync(int assignmentId);
        Task<IEnumerable<SalesInvoiceAssignmentRow>> GetAssignmentsForInvoiceAsync(int invoiceId);
    }
}
