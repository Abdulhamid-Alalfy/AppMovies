using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        // Method called by server to send notification to all connected clients
        public async Task SendNotificationToAll(string title, string message, string link, string type)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message, link, type);
        }

        // Method called by server to send notification to specific user
        public async Task SendNotificationToUser(string userId, string title, string message, string link, string type)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, message, link, type);
        }

        // Called when a client connects
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
