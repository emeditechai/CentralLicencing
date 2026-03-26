using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEmployeeDepartmentRepository
    {
        Task<IEnumerable<EmployeeDepartmentMaster>> GetAllAsync();
        Task<IEnumerable<EmployeeDepartmentMaster>> GetAllActiveAsync();
        Task<EmployeeDepartmentMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(EmployeeDepartmentMaster department);
        Task<bool> UpdateAsync(EmployeeDepartmentMaster department);
        Task<bool> DeleteAsync(int id);
    }
}