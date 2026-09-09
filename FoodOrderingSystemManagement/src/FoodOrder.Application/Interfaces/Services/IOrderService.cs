using FoodOrder.Application.DTOs.Order;

namespace FoodOrder.Application.Interfaces.Services;

public interface IOrderService
{
    Task<ReceiptDto> CreateOrderAsync(CreateOrderDto dto, int cashierId);
    Task<PublicOrderConfirmationDto> PlacePublicOrderAsync(PublicOrderDto dto);
    /// <summary>Pass branchId to let an org-wide admin (or SuperAdmin) narrow to one specific branch instead of every branch combined.</summary>
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(int? branchId = null);
    Task<IReadOnlyList<OrderDto>> GetOrdersByUserAsync(int userId);
    Task<IReadOnlyList<OrderDto>> GetPendingOrdersAsync();
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task CancelOrderAsync(int orderId, int requestingUserId);
    Task TransferTableAsync(int orderId, int newTableId);
    Task TransferToTakeawayAsync(int orderId);
    Task<OrderDto> AddItemsToOrderAsync(int orderId, AddItemsToOrderDto dto);
    Task<ReceiptDto> GenerateBillAsync(int orderId, decimal? discount = null);
    Task ConfirmPaymentAsync(int orderId);
    Task<OrderDto> MarkOrderReadyAsync(int orderId);
    Task<OrderDto> MarkOrderServedAsync(int orderId);
    Task ReopenOrderAsync(int orderId);
    Task DeleteOrderAsync(int orderId);
    Task KitchenRejectOrderAsync(int orderId, string? reason);
    Task<(bool orderCancelled, List<string> removedItemNames)> RejectOrderItemsAsync(int orderId, List<int> foodItemIds, string? reason);
}
