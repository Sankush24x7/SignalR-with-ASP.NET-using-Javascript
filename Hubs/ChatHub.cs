using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override Task OnConnectedAsync()
        {
            ConnectedUsers[Context.ConnectionId] = Context.User?.Identity?.Name ?? "Anonymous";
            Clients.All.SendAsync("UserConnected", ConnectedUsers[Context.ConnectionId]);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var username))
            {
                Clients.All.SendAsync("UserDisconnected", username);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendPrivateMessage(string toConnectionId, string message)
        {
            await Clients.Client(toConnectionId).SendAsync("ReceivePrivateMessage", ConnectedUsers[Context.ConnectionId], message);
        }

        public async Task Typing(string user)
        {
            await Clients.Others.SendAsync("UserTyping", user);
        }
    }
}
