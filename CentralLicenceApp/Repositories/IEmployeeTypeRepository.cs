using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEmployeeTypeRepository
    {
        Task<IEnumerable<EmployeeTypeMaster>> GetAllAsync();
        Task<IEnumerable<EmployeeTypeMaster>> GetAllActiveAsync();
        Task<EmployeeTypeMaster?> GetByIdAsync(int id);
    }
}