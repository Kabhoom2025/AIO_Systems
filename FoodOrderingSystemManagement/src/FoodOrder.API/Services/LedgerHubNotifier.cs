using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FoodOrder.API.Services;

public class LedgerHubNotifier : ILedgerNotifier
{
    private readonly IHubContext<OrderHub> _hub;

    public LedgerHubNotifier(IHubContext<OrderHub> hub) => _hub = hub;

    public Task NotifyEntryCreatedAsync(LedgerEntryDTO entry)
        => _hub.Clients.All.SendAsync("LedgerEntryCreated", entry);
}
