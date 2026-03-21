using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication.Hubs.FilterAttribute;

namespace RealTimeCommunication.Hubs;

[Authorize] // garante que só usuários autenticados entram
public class ChatHub : Hub
{
    [LanguageFilter(0)] // filtro customizado
    public async Task SendMessage(string user, string message)
    {
        // Envia mensagem para todos
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    [Authorize("Admin")] // exemplo de autorização por política
    public Task SendPrivateMessage(string user, string message)
    {
        return Clients.User(user).SendAsync("ReceiveMessage", message);
    }

    public async Task AddToGroup(string groupName)
    {
        // Adiciona usuário ao grupo
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Notifica grupo
        await Clients.Group(groupName)
            .SendAsync("Send", $"{Context.UserIdentifier} entrou no grupo {groupName}");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName)
            .SendAsync("Send", $"{Context.UserIdentifier} saiu do grupo {groupName}");
    }
}