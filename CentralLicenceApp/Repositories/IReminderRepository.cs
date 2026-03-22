using System.Threading.Tasks;

namespace CentralLicenceApp.Repositories
{
    public interface IReminderRepository
    {
        Task<bool> WasSentTodayAsync(int licenseId, string reminderType);
        Task RecordAsync(int licenseId, string reminderType, string toEmail);
    }
}
