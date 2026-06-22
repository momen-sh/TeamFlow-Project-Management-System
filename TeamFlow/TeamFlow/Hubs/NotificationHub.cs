using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TeamFlow.Authorization;

namespace TeamFlow.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();
            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId.Value));
            }

            await base.OnConnectedAsync();
        }

        public static string UserGroup(int userId) => $"user:{userId}";
    }
}
