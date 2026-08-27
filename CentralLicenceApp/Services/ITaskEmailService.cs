using CentralLicenceApp.Models;

namespace CentralLicenceApp.Services
{
    public interface ITaskEmailService
    {
        /// <summary>Sends a "Task Assigned to You" notification to the assigned user.</summary>
        Task NotifyTaskAssignedAsync(DailyTaskLog task, string assigneeName, string assigneeEmail);

        /// <summary>
        /// Sends a "Task Updated" notification to the assigned user.
        /// <paramref name="changedFields"/> is a list of human-readable change summaries
        /// (e.g. "Title: Old Value → New Value") shown in the email body.
        /// </summary>
        Task NotifyTaskUpdatedAsync(DailyTaskLog task, string assigneeName, string assigneeEmail,
            IEnumerable<string>? changedFields = null);

        /// <summary>Sends a "You were mentioned" notification to the tagged user.</summary>
        Task NotifyTaskCommentMentionAsync(DailyTaskLog task, string senderName, string mentionedName, string mentionedEmail, string commentText);
    }
}
