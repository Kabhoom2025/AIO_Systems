using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>Returns all orders with Cashier and OrderItems (with FoodItem) loaded.</summary>
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync();

    /// <summary>Returns orders placed by a specific cashier/waiter user.</summary>
    Task<IReadOnlyList<Order>> GetByUserAsync(int userId);

    /// <summary>Returns a single order with full navigation properties loaded.</summary>
    Task<Order?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Generates the next sequential order number (ORD000001, ORD000002...).
    /// Reads the last inserted order number from the database to determine the next value.
    /// The unique index on OrderNumber handles the rare concurrent-insert edge case.
    /// </summary>
    Task<string> GenerateOrderNumberAsync();

    /// <summary>Permanently removes specific OrderItem rows from the database.</summary>
    void RemoveOrderItems(IEnumerable<OrderItem> items);
}
