using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IUserPushSubscriptionRepository
    {
        Task UpsertAsync(UserPushSubscription subscription);
        Task DeactivateAsync(int userId, string endpoint);
        Task DeactivateByEndpointAsync(string endpoint);
        Task<IEnumerable<UserPushSubscription>> GetActiveByUserIdsAsync(IEnumerable<int> userIds);
    }
}