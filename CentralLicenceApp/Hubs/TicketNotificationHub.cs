using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CentralLicenceApp.Hubs
{
    [Authorize]
    public class TicketNotificationHub : Hub
    {
    }
}
