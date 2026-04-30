using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IAppUploadRepository
    {
        Task<IEnumerable<AppUploadLog>> GetAllAsync();
        Task<AppUploadLog?> GetLatestByPlatformAsync(string platform);
        Task<int> AddAsync(AppUploadLog log);
    }
}
