using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEmployeeDesignationRepository
    {
        Task<IEnumerable<EmployeeDesignationMaster>> GetAllAsync();
        Task<IEnumerable<EmployeeDesignationMaster>> GetAllActiveAsync();
        Task<EmployeeDesignationMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(EmployeeDesignationMaster designation);
        Task<bool> UpdateAsync(EmployeeDesignationMaster designation);
        Task<bool> DeleteAsync(int id);
    }
}