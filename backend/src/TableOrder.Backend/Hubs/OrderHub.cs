using Microsoft.AspNetCore.SignalR;

namespace TableOrder.Backend.Hubs;

public class OrderHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }

    public async Task JoinKitchenGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Kitchen");
        await Clients.Caller.SendAsync("JoinedKitchen");
    }

    public async Task JoinTableGroup(int tableNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Table_{tableNumber}");
        await Clients.Caller.SendAsync("JoinedTable", tableNumber);
    }

    public async Task LeaveKitchenGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Kitchen");
        await Clients.Caller.SendAsync("LeftKitchen");
    }

    public async Task LeaveTableGroup(int tableNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Table_{tableNumber}");
        await Clients.Caller.SendAsync("LeftTable", tableNumber);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("Disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
