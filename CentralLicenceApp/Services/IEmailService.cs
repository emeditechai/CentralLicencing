using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentralLicenceApp.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody, string? emailType = null);
        Task SendWithAttachmentAsync(string toEmail, string toName, string subject, string htmlBody,
            byte[] attachmentBytes, string attachmentFileName, string? emailType = null);
        Task SendTemplatedAsync(string templateKey, string toEmail, string toName,
            Dictionary<string, string> placeholders);
        Task<(string Subject, string Body)?> ResolveTemplateAsync(string templateKey, Dictionary<string, string> placeholders);
    }
}
