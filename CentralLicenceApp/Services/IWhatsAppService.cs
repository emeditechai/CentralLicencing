using System.Threading;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Services
{
    public interface IWhatsAppService
    {
        /// <summary>
        /// Sends one WhatsApp message through WaSender and always writes a WhatsAppLog row
        /// (except when no configuration / token exists or the master switch is off).
        /// Never throws: failures come back in the result.
        /// </summary>
        Task<WhatsAppSendResult> SendTextMessageAsync(string toPhone, string text, string moduleCode,
            int? moduleRefId = null, string? documentUrl = null, string? fileName = null,
            string? sentBy = null, bool ignoreMasterSwitch = false, string? overrideApiKey = null,
            CancellationToken ct = default);

        /// <summary>Uploads a file to WaSender and returns its public URL, or null on failure.</summary>
        Task<string?> UploadMediaAsync(byte[] content, string mimeType, CancellationToken ct = default);

        string ProtectApiKey(string plainKey);

        string FormatPhoneNumber(string raw, string? defaultCountryCode);
    }
}
