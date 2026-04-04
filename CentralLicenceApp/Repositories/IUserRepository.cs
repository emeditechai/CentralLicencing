using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserMaster>> GetAllAsync();
        Task<(IEnumerable<UserMaster> Items, int TotalCount)> GetPagedAsync(string? search, string? status, int? roleId, int page, int pageSize);
        Task<UserMaster?> GetByIdAsync(int id);
        Task<UserMaster?> GetByUsernameAsync(string username);
        Task<int> CreateAsync(UserMaster user);
        Task<bool> UpdateAsync(UserMaster user);
        Task<bool> UpdatePasswordAsync(int id, string passwordHash);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> CheckEmployeeCodeUniqueAsync(string employeeCode, int? excludeUserId = null);
        Task<IEnumerable<UserMaster>> GetEmployeesAsync();
        Task<IEnumerable<UserMaster>> GetCoreMembersAsync();
        Task<IEnumerable<UserMaster>> GetSignatoryUsersAsync();
        Task<IReadOnlyCollection<int>> GetSelfAndSubordinateIdsAsync(int userId);
    }

    public interface IRoleRepository
    {
        Task<IEnumerable<RoleMaster>> GetAllAsync();
        Task<RoleMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(RoleMaster role);
        Task<bool> UpdateAsync(RoleMaster role);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

