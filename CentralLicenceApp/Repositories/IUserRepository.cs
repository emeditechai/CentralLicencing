using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserMaster>> GetAllAsync();
        Task<UserMaster?> GetByIdAsync(int id);
        Task<UserMaster?> GetByUsernameAsync(string username);
        Task<int> CreateAsync(UserMaster user);
        Task<bool> UpdateAsync(UserMaster user);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> CheckEmployeeCodeUniqueAsync(string employeeCode, int? excludeUserId = null);
    }

    public interface IRoleRepository
    {
        Task<IEnumerable<RoleMaster>> GetAllAsync();
        Task<RoleMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(RoleMaster role);
        Task<bool> UpdateAsync(RoleMaster role);
        Task<bool> DeleteAsync(int id);
    }
}

