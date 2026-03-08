using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication.Hubs.FilterAttribute;

namespace RealTimeCommunication.Hubs;

public class ChatHub : Hub
{
    [LanguageFilter(0)]
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public Task SendPrivateMessage(string user, string message)
    {
        return Clients.User(user).SendAsync("ReceiveMessage", message);
    }

    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    }
}
