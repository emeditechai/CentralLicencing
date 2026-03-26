using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IExpenseCategoryRepository
    {
        Task<IEnumerable<ExpenseCategoryMaster>> GetAllAsync();
        Task<IEnumerable<ExpenseCategoryMaster>> GetAllActiveAsync();
        Task<ExpenseCategoryMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(ExpenseCategoryMaster expenseCategory);
        Task<bool> UpdateAsync(ExpenseCategoryMaster expenseCategory);
        Task<bool> DeleteAsync(int id);
    }
}