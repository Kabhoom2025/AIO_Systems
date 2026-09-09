using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Constants;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync() =>
        await _context.Orders
            .Include(o => o.Cashier)
            .Include(o => o.Branch)
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<IReadOnlyList<Order>> GetByUserAsync(int userId) =>
        await _context.Orders
            .Include(o => o.Cashier)
            .Include(o => o.Branch)
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
            .Where(o => o.CashierId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> GetByIdWithDetailsAsync(int id) =>
        await _context.Orders
            .Include(o => o.Cashier)
            .Include(o => o.Branch)
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
            .FirstOrDefaultAsync(o => o.Id == id);

    public void RemoveOrderItems(IEnumerable<OrderItem> items)
        => _context.Set<OrderItem>().RemoveRange(items);

    public async Task<string> GenerateOrderNumberAsync()
    {
        var lastOrderNumber = await _context.Orders
            .OrderByDescending(o => o.Id)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastOrderNumber != null &&
            lastOrderNumber.StartsWith(OrderConstants.OrderNumberPrefix))
        {
            var numericPart = lastOrderNumber[OrderConstants.OrderNumberPrefix.Length..];
            if (int.TryParse(numericPart, out int parsed))
                nextNumber = parsed + 1;
        }

        return $"{OrderConstants.OrderNumberPrefix}{nextNumber.ToString().PadLeft(OrderConstants.OrderNumberPadding, '0')}";
    }
}
