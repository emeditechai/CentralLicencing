using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ISalesInvoiceAssignmentRepository
    {
        Task<IEnumerable<SalesInvoiceAssignmentRow>> GetAssignmentsAsync(int? salesUserId, DateTime? from, DateTime? to);
        Task<IEnumerable<SalesInvoiceAssignmentRow>> GetUnassignedInvoicesAsync(DateTime? from, DateTime? to);
        Task<bool> AssignAsync(int invoiceId, int salesUserId, int? productId, int assignedById);
        Task<bool> UnassignAsync(int invoiceId);
        Task<bool> ReassignAsync(int invoiceId, int newSalesUserId, int? productId, int assignedById);
    }
}
