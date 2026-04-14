using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CentralLicenceApp.Hubs
{
    [Authorize]
    public class TicketNotificationHub : Hub
    {
        public async Task JoinTicketGroup(int ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task LeaveTicketGroup(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task SendTyping(int ticketId)
        {
            var userName = Context.User?.FindFirst("FullName")?.Value
                        ?? Context.User?.Identity?.Name
                        ?? "Someone";
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.OthersInGroup($"ticket-{ticketId}")
                .SendAsync("UserTyping", new { userName, userId, ticketId });
        }

        public async Task MarkAsRead(int ticketId)
        {
            var userName = Context.User?.FindFirst("FullName")?.Value
                        ?? Context.User?.Identity?.Name
                        ?? "Someone";
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.OthersInGroup($"ticket-{ticketId}")
                .SendAsync("TicketRead", new { userName, userId, ticketId, readAt = DateTime.UtcNow });
        }
    }
}
