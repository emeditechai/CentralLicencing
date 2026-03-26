using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Hubs;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace CentralLicenceApp.Services
{
    public interface IExpenseBrowserNotificationService
    {
        Task NotifyExpenseRequestSubmittedAsync(ExpenseRequest request, UserMaster submittedByUser);
        Task NotifyExpenseRequestApprovedAsync(ExpenseRequest request, UserMaster approvedByUser);
        Task NotifyExpenseRequestRejectedAsync(ExpenseRequest request, UserMaster rejectedByUser);
        Task NotifyReimbursementStartedAsync(ExpenseRequest request, UserMaster financeUser);
        Task NotifySettlementCompletedAsync(ExpenseRequest request, UserMaster financeUser);
    }

    public class ExpenseBrowserNotificationService : IExpenseBrowserNotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserPushSubscriptionRepository _subscriptionRepository;
        private readonly IHubContext<ExpenseNotificationHub> _hubContext;
        private readonly PushNotificationSettings _pushSettings;
        private readonly ILogger<ExpenseBrowserNotificationService> _logger;

        public ExpenseBrowserNotificationService(
            IUserRepository userRepository,
            IUserPushSubscriptionRepository subscriptionRepository,
            IHubContext<ExpenseNotificationHub> hubContext,
            IOptions<PushNotificationSettings> pushSettings,
            ILogger<ExpenseBrowserNotificationService> logger)
        {
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
            _hubContext = hubContext;
            _pushSettings = pushSettings.Value;
            _logger = logger;
        }

        public async Task NotifyExpenseRequestSubmittedAsync(ExpenseRequest request, UserMaster submittedByUser)
        {
            if (request == null || submittedByUser == null)
            {
                return;
            }

            var recipients = new Dictionary<int, BrowserExpenseNotification>();

            if (submittedByUser.ManagerId.HasValue)
            {
                var manager = await _userRepository.GetByIdAsync(submittedByUser.ManagerId.Value);
                if (manager != null && manager.IsActive && manager.Id != submittedByUser.Id)
                {
                    recipients[manager.Id] = new BrowserExpenseNotification
                    {
                        Id = $"expense-request-submitted-manager-{request.Id}",
                        Title = "Expense approval required",
                        Message = "A new expense or advance request is awaiting your review.",
                        TargetUrl = $"/ExpenseRequest/Details/{request.Id}",
                        RequestNumber = request.RequestNumber
                    };
                }
            }

            var coreMembers = (await _userRepository.GetCoreMembersAsync())
                .Where(user => user.IsActive && user.Id != submittedByUser.Id)
                .GroupBy(user => user.Id)
                .Select(group => group.First());

            foreach (var coreMember in coreMembers)
            {
                if (!recipients.ContainsKey(coreMember.Id))
                {
                    recipients[coreMember.Id] = new BrowserExpenseNotification
                    {
                        Id = $"expense-request-submitted-core-{request.Id}",
                        Title = "Expense request submitted",
                        Message = "A new expense or advance request was submitted. Open the dashboard to continue.",
                        TargetUrl = "/Dashboard/Index",
                        RequestNumber = request.RequestNumber
                    };
                }
            }

            await NotifyRecipientsAsync(request.Id, recipients);
        }

        public async Task NotifyExpenseRequestApprovedAsync(ExpenseRequest request, UserMaster approvedByUser)
        {
            var recipients = new Dictionary<int, BrowserExpenseNotification>();

            if (request.EmployeeId > 0)
            {
                recipients[request.EmployeeId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-approved-employee-{request.Id}",
                    Category = "expense-request-approved",
                    Title = "Expense request approved",
                    Message = "Your expense or advance request has been approved.",
                    TargetUrl = $"/ExpenseRequest/Details/{request.Id}",
                    RequestNumber = request.RequestNumber
                };
            }

            foreach (var financeUserId in await GetFinanceRecipientIdsAsync(approvedByUser.Id))
            {
                recipients[financeUserId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-approved-finance-{request.Id}-{financeUserId}",
                    Category = "expense-request-ready-for-reimbursement",
                    Title = "Request ready for reimbursement",
                    Message = "An approved expense or advance request is ready for finance processing.",
                    TargetUrl = "/ExpenseRequest/FinanceDesk",
                    RequestNumber = request.RequestNumber
                };
            }

            await NotifyRecipientsAsync(request.Id, recipients);
        }

        public async Task NotifyExpenseRequestRejectedAsync(ExpenseRequest request, UserMaster rejectedByUser)
        {
            if (request.EmployeeId <= 0)
            {
                return;
            }

            await NotifyRecipientsAsync(request.Id, new Dictionary<int, BrowserExpenseNotification>
            {
                [request.EmployeeId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-rejected-employee-{request.Id}",
                    Category = "expense-request-rejected",
                    Title = "Expense request rejected",
                    Message = "Your expense or advance request was rejected. Review the remarks in the request details.",
                    TargetUrl = $"/ExpenseRequest/Details/{request.Id}",
                    RequestNumber = request.RequestNumber
                }
            });
        }

        public async Task NotifyReimbursementStartedAsync(ExpenseRequest request, UserMaster financeUser)
        {
            var recipients = new Dictionary<int, BrowserExpenseNotification>();

            if (request.EmployeeId > 0)
            {
                recipients[request.EmployeeId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-reimbursement-employee-{request.Id}",
                    Category = "expense-request-reimbursement-started",
                    Title = "Reimbursement started",
                    Message = "Finance has started reimbursement processing for your request.",
                    TargetUrl = $"/ExpenseRequest/Details/{request.Id}",
                    RequestNumber = request.RequestNumber
                };
            }

            foreach (var financeUserId in await GetFinanceRecipientIdsAsync(financeUser.Id))
            {
                recipients[financeUserId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-reimbursement-finance-{request.Id}-{financeUserId}",
                    Category = "expense-request-reimbursement-started",
                    Title = "Reimbursement in progress",
                    Message = "A reimbursement process has started for an expense or advance request.",
                    TargetUrl = "/ExpenseRequest/FinanceDesk",
                    RequestNumber = request.RequestNumber
                };
            }

            await NotifyRecipientsAsync(request.Id, recipients);
        }

        public async Task NotifySettlementCompletedAsync(ExpenseRequest request, UserMaster financeUser)
        {
            var recipients = new Dictionary<int, BrowserExpenseNotification>();

            if (request.EmployeeId > 0)
            {
                recipients[request.EmployeeId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-settlement-employee-{request.Id}",
                    Category = "expense-request-settled",
                    Title = "Request settled",
                    Message = "Your expense or advance request has been settled.",
                    TargetUrl = $"/ExpenseRequest/SettlementReceipt/{request.Id}",
                    RequestNumber = request.RequestNumber
                };
            }

            foreach (var financeUserId in await GetFinanceRecipientIdsAsync(financeUser.Id))
            {
                recipients[financeUserId] = new BrowserExpenseNotification
                {
                    Id = $"expense-request-settlement-finance-{request.Id}-{financeUserId}",
                    Category = "expense-request-settled",
                    Title = "Request settled",
                    Message = "A finance settlement has been completed for an expense or advance request.",
                    TargetUrl = "/ExpenseRequest/FinanceDesk",
                    RequestNumber = request.RequestNumber
                };
            }

            await NotifyRecipientsAsync(request.Id, recipients);
        }

        private async Task NotifyRecipientsAsync(int requestId, Dictionary<int, BrowserExpenseNotification> recipients)
        {
            if (recipients.Count == 0)
            {
                return;
            }

            var recipientIds = recipients.Keys.ToList();
            var subscriptions = (await _subscriptionRepository.GetActiveByUserIdsAsync(recipientIds))
                .GroupBy(subscription => subscription.UserId)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var recipient in recipients)
            {
                try
                {
                    await _hubContext.Clients.User(recipient.Key.ToString())
                        .SendAsync("ExpenseNotificationReceived", recipient.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send SignalR expense notification for request {RequestId} to user {UserId}",
                        requestId,
                        recipient.Key);
                }

                if (!subscriptions.TryGetValue(recipient.Key, out var userSubscriptions))
                {
                    continue;
                }

                foreach (var subscription in userSubscriptions)
                {
                    await SendWebPushAsync(requestId, recipient.Key, recipient.Value, subscription);
                }
            }
        }

        private async Task SendWebPushAsync(int requestId, int userId, BrowserExpenseNotification payload, UserPushSubscription subscription)
        {
            if (!_pushSettings.IsConfigured)
            {
                return;
            }

            try
            {
                var client = new WebPushClient();
                var pushSubscription = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
                var vapidDetails = new VapidDetails(_pushSettings.Subject, _pushSettings.PublicKey, _pushSettings.PrivateKey);
                await client.SendNotificationAsync(pushSubscription, System.Text.Json.JsonSerializer.Serialize(payload), vapidDetails);
            }
            catch (WebPushException ex) when ((int?)ex.StatusCode == 404 || (int?)ex.StatusCode == 410)
            {
                await _subscriptionRepository.DeactivateByEndpointAsync(subscription.Endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send web push expense notification for request {RequestId} to user {UserId}",
                    requestId,
                    userId);
            }
        }

        private async Task<List<int>> GetFinanceRecipientIdsAsync(int excludedUserId)
        {
            var users = (await _userRepository.GetAllAsync())
                .Where(user => user.IsActive)
                .Where(user => user.Id != excludedUserId)
                .Where(user => user.Roles.Any(role => string.Equals(role.RoleName, "Finance", StringComparison.OrdinalIgnoreCase))
                    || string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Id)
                .Distinct()
                .ToList();

            return users;
        }
    }

    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}