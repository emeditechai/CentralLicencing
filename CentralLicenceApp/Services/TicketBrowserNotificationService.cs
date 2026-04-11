using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Hubs;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace CentralLicenceApp.Services
{
    public class TicketBrowserNotificationService : ITicketBrowserNotificationService
    {
        private readonly IUserPushSubscriptionRepository _subscriptionRepository;
        private readonly IHubContext<TicketNotificationHub> _hubContext;
        private readonly PushNotificationSettings _pushSettings;
        private readonly string _connectionString;
        private readonly ILogger<TicketBrowserNotificationService> _logger;

        public TicketBrowserNotificationService(
            IUserPushSubscriptionRepository subscriptionRepository,
            IHubContext<TicketNotificationHub> hubContext,
            IOptions<PushNotificationSettings> pushSettings,
            string connectionString,
            ILogger<TicketBrowserNotificationService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _hubContext = hubContext;
            _pushSettings = pushSettings.Value;
            _connectionString = connectionString;
            _logger = logger;
        }

        // ── 1. Ticket Created → Client (confirmation) + Assigned Agent (if any) + Ticket Admins ──
        public async Task NotifyTicketCreatedAsync(HelpDeskTicket ticket)
        {
            try
            {
                var recipients = new Dictionary<int, BrowserTicketNotification>();
                var detailsUrl = $"/HelpDeskTicket/Details/{ticket.Id}";

                // Notify the ticket creator (confirmation)
                recipients[ticket.CreatedById] = new BrowserTicketNotification
                {
                    Id = $"ticket-created-client-{ticket.Id}",
                    Category = "ticket-created",
                    Title = "Ticket Created Successfully",
                    Message = $"Your ticket {ticket.TicketNumber} has been created and our team will review it shortly.",
                    TargetUrl = detailsUrl,
                    TicketNumber = ticket.TicketNumber
                };

                // Notify assigned agent (if assigned at creation)
                if (ticket.AssignedToId.HasValue && ticket.AssignedToId.Value != ticket.CreatedById)
                {
                    recipients[ticket.AssignedToId.Value] = new BrowserTicketNotification
                    {
                        Id = $"ticket-created-agent-{ticket.Id}",
                        Category = "ticket-created",
                        Title = "New Ticket Assigned",
                        Message = $"Ticket {ticket.TicketNumber} – \"{Truncate(ticket.Subject, 60)}\" has been assigned to you.",
                        TargetUrl = detailsUrl,
                        TicketNumber = ticket.TicketNumber
                    };
                }

                // Notify all Ticket Admins
                var adminIds = await GetTicketAdminUserIdsAsync();
                foreach (var adminId in adminIds)
                {
                    if (!recipients.ContainsKey(adminId))
                    {
                        recipients[adminId] = new BrowserTicketNotification
                        {
                            Id = $"ticket-created-admin-{ticket.Id}",
                            Category = "ticket-created",
                            Title = "New Support Ticket",
                            Message = $"New ticket {ticket.TicketNumber} – \"{Truncate(ticket.Subject, 60)}\" requires attention.",
                            TargetUrl = "/HelpDeskTicket/Index",
                            TicketNumber = ticket.TicketNumber
                        };
                    }
                }

                await NotifyRecipientsAsync(ticket.Id, recipients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification for ticket created {TicketId}.", ticket.Id);
            }
        }

        // ── 2. Ticket Assigned → Assigned Agent + Ticket Admins ──
        public async Task NotifyTicketAssignedAsync(HelpDeskTicket ticket, string assigneeName, int assigneeId)
        {
            try
            {
                var recipients = new Dictionary<int, BrowserTicketNotification>();
                var detailsUrl = $"/HelpDeskTicket/Details/{ticket.Id}";

                // Notify the assigned agent
                recipients[assigneeId] = new BrowserTicketNotification
                {
                    Id = $"ticket-assigned-agent-{ticket.Id}",
                    Category = "ticket-assigned",
                    Title = "Ticket Assigned to You",
                    Message = $"Ticket {ticket.TicketNumber} – \"{Truncate(ticket.Subject, 60)}\" has been assigned to you.",
                    TargetUrl = detailsUrl,
                    TicketNumber = ticket.TicketNumber
                };

                // Notify Ticket Admins (except the assignee)
                var adminIds = await GetTicketAdminUserIdsAsync();
                foreach (var adminId in adminIds)
                {
                    if (!recipients.ContainsKey(adminId))
                    {
                        recipients[adminId] = new BrowserTicketNotification
                        {
                            Id = $"ticket-assigned-admin-{ticket.Id}",
                            Category = "ticket-assigned",
                            Title = "Ticket Assignment Update",
                            Message = $"Ticket {ticket.TicketNumber} has been assigned to {assigneeName}.",
                            TargetUrl = detailsUrl,
                            TicketNumber = ticket.TicketNumber
                        };
                    }
                }

                await NotifyRecipientsAsync(ticket.Id, recipients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification for ticket assigned {TicketId}.", ticket.Id);
            }
        }

        // ── 3. Status Changed → Client (creator) + Assigned Agent + Ticket Admins ──
        public async Task NotifyStatusChangedAsync(HelpDeskTicket ticket, string oldStatus, string newStatus)
        {
            try
            {
                var recipients = new Dictionary<int, BrowserTicketNotification>();
                var detailsUrl = $"/HelpDeskTicket/Details/{ticket.Id}";

                // Notify ticket creator
                recipients[ticket.CreatedById] = new BrowserTicketNotification
                {
                    Id = $"ticket-status-changed-client-{ticket.Id}-{newStatus}",
                    Category = "ticket-status-changed",
                    Title = newStatus == "Closed" ? "Ticket Closed" : "Ticket Status Updated",
                    Message = newStatus == "Closed"
                        ? $"Your ticket {ticket.TicketNumber} has been closed."
                        : $"Your ticket {ticket.TicketNumber} status changed to \"{newStatus}\".",
                    TargetUrl = detailsUrl,
                    TicketNumber = ticket.TicketNumber
                };

                // Notify assigned agent
                if (ticket.AssignedToId.HasValue && ticket.AssignedToId.Value != ticket.CreatedById)
                {
                    recipients[ticket.AssignedToId.Value] = new BrowserTicketNotification
                    {
                        Id = $"ticket-status-changed-agent-{ticket.Id}-{newStatus}",
                        Category = "ticket-status-changed",
                        Title = "Ticket Status Changed",
                        Message = $"Ticket {ticket.TicketNumber} status changed from \"{oldStatus}\" to \"{newStatus}\".",
                        TargetUrl = detailsUrl,
                        TicketNumber = ticket.TicketNumber
                    };
                }

                // Notify Ticket Admins
                var adminIds = await GetTicketAdminUserIdsAsync();
                foreach (var adminId in adminIds)
                {
                    if (!recipients.ContainsKey(adminId))
                    {
                        recipients[adminId] = new BrowserTicketNotification
                        {
                            Id = $"ticket-status-changed-admin-{ticket.Id}-{newStatus}",
                            Category = "ticket-status-changed",
                            Title = "Ticket Status Changed",
                            Message = $"Ticket {ticket.TicketNumber} status: \"{oldStatus}\" → \"{newStatus}\".",
                            TargetUrl = detailsUrl,
                            TicketNumber = ticket.TicketNumber
                        };
                    }
                }

                await NotifyRecipientsAsync(ticket.Id, recipients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification for ticket status change {TicketId}.", ticket.Id);
            }
        }

        // ── 4. New Reply → Client (public only) + Assigned Agent + Ticket Admins ──
        public async Task NotifyNewReplyAsync(HelpDeskTicket ticket, string replierName, string messageSnippet, bool isInternal)
        {
            try
            {
                var recipients = new Dictionary<int, BrowserTicketNotification>();
                var detailsUrl = $"/HelpDeskTicket/Details/{ticket.Id}";
                var snippet = Truncate(messageSnippet, 80);

                // Public reply → notify ticket creator
                if (!isInternal)
                {
                    recipients[ticket.CreatedById] = new BrowserTicketNotification
                    {
                        Id = $"ticket-reply-client-{ticket.Id}-{DateTime.UtcNow.Ticks}",
                        Category = "ticket-reply",
                        Title = "New Reply on Your Ticket",
                        Message = $"{replierName} replied on {ticket.TicketNumber}: \"{snippet}\"",
                        TargetUrl = detailsUrl,
                        TicketNumber = ticket.TicketNumber
                    };
                }

                // Notify assigned agent (if they are not the replier)
                if (ticket.AssignedToId.HasValue && !recipients.ContainsKey(ticket.AssignedToId.Value))
                {
                    recipients[ticket.AssignedToId.Value] = new BrowserTicketNotification
                    {
                        Id = $"ticket-reply-agent-{ticket.Id}-{DateTime.UtcNow.Ticks}",
                        Category = isInternal ? "ticket-internal-note" : "ticket-reply",
                        Title = isInternal ? "New Internal Note" : "New Client Reply",
                        Message = isInternal
                            ? $"{replierName} added an internal note on {ticket.TicketNumber}."
                            : $"{replierName} replied on {ticket.TicketNumber}: \"{snippet}\"",
                        TargetUrl = detailsUrl,
                        TicketNumber = ticket.TicketNumber
                    };
                }

                // Notify Ticket Admins
                var adminIds = await GetTicketAdminUserIdsAsync();
                foreach (var adminId in adminIds)
                {
                    if (!recipients.ContainsKey(adminId))
                    {
                        recipients[adminId] = new BrowserTicketNotification
                        {
                            Id = $"ticket-reply-admin-{ticket.Id}-{DateTime.UtcNow.Ticks}",
                            Category = isInternal ? "ticket-internal-note" : "ticket-reply",
                            Title = isInternal ? "New Internal Note" : "New Ticket Reply",
                            Message = isInternal
                                ? $"{replierName} added an internal note on {ticket.TicketNumber}."
                                : $"{replierName} replied on {ticket.TicketNumber}: \"{snippet}\"",
                            TargetUrl = detailsUrl,
                            TicketNumber = ticket.TicketNumber
                        };
                    }
                }

                await NotifyRecipientsAsync(ticket.Id, recipients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification for ticket reply {TicketId}.", ticket.Id);
            }
        }

        // ── Core: Send to all recipients via SignalR + Web Push ──
        private async Task NotifyRecipientsAsync(int ticketId, Dictionary<int, BrowserTicketNotification> recipients)
        {
            if (recipients.Count == 0) return;

            // Step 1: SignalR (real-time in-app)
            foreach (var recipient in recipients)
            {
                try
                {
                    await _hubContext.Clients.User(recipient.Key.ToString())
                        .SendAsync("TicketNotificationReceived", recipient.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send SignalR ticket notification for ticket {TicketId} to user {UserId}.",
                        ticketId, recipient.Key);
                }
            }

            // Step 2: Web Push (VAPID — OS-level notifications)
            if (!_pushSettings.IsConfigured) return;

            Dictionary<int, List<UserPushSubscription>> subscriptions;
            try
            {
                var recipientIds = recipients.Keys.ToList();
                subscriptions = (await _subscriptionRepository.GetActiveByUserIdsAsync(recipientIds))
                    .GroupBy(s => s.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to load push subscriptions for ticket {TicketId}. SignalR alerts were still sent.",
                    ticketId);
                return;
            }

            foreach (var recipient in recipients)
            {
                if (!subscriptions.TryGetValue(recipient.Key, out var userSubscriptions))
                    continue;

                foreach (var subscription in userSubscriptions)
                {
                    await SendWebPushAsync(ticketId, recipient.Key, recipient.Value, subscription);
                }
            }
        }

        private async Task SendWebPushAsync(int ticketId, int userId, BrowserTicketNotification payload, UserPushSubscription subscription)
        {
            if (!_pushSettings.IsConfigured) return;

            try
            {
                var client = new WebPushClient();
                var pushSubscription = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
                var vapidDetails = new VapidDetails(_pushSettings.Subject, _pushSettings.PublicKey, _pushSettings.PrivateKey);
                await client.SendNotificationAsync(
                    pushSubscription,
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    vapidDetails);
            }
            catch (WebPushException ex) when ((int?)ex.StatusCode == 404 || (int?)ex.StatusCode == 410)
            {
                await _subscriptionRepository.DeactivateByEndpointAsync(subscription.Endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send web push ticket notification for ticket {TicketId} to user {UserId}.",
                    ticketId, userId);
            }
        }

        // ── Helpers ──

        private async Task<List<int>> GetTicketAdminUserIdsAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            var results = await conn.QueryAsync<int>(@"
                SELECT DISTINCT u.Id
                FROM UserMaster u
                INNER JOIN UserRoleMap urm ON urm.UserId = u.Id
                INNER JOIN RoleMaster r ON r.Id = urm.RoleId
                WHERE u.IsActive = 1
                  AND r.RoleName IN ('Ticket Admin', 'Administrator')");
            return results.ToList();
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text[..maxLength] + "…";
        }
    }
}
