namespace FoodOrder.Application.Interfaces.Services;

public interface ISmsService
{
    Task SendOrderConfirmationAsync(string phone, string orderNumber, string itemsSummary, decimal grandTotal, string restaurantName);
    Task SendOrderCancellationAsync(string phone, string orderNumber, decimal grandTotal, string restaurantName);
}
