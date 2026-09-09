using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface ILedgerNotifier
{
    Task NotifyEntryCreatedAsync(LedgerEntryDTO entry);
}
