using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Customer>> GetAllAsync() =>
        await _context.Customers.OrderByDescending(c => c.TotalSpend).ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _context.Customers.FindAsync(id);

    public async Task<Customer?> GetByPhoneAsync(string phone) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.Phone == phone && c.IsActive);

    public async Task<IReadOnlyList<PointsTransaction>> GetTransactionsAsync(int customerId) =>
        await _context.PointsTransactions
            .Where(pt => pt.CustomerId == customerId)
            .OrderByDescending(pt => pt.CreatedDate)
            .ToListAsync();

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(int customerId) =>
        await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<IReadOnlyList<SavedAddress>> GetAddressesAsync(int customerId) =>
        await _context.SavedAddresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .ToListAsync();

    public async Task<SavedAddress?> GetAddressByIdAsync(int addressId) =>
        await _context.SavedAddresses.FindAsync(addressId);

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(PointsTransaction txn)
    {
        _context.PointsTransactions.Add(txn);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null) =>
        await _context.Customers.AnyAsync(c => c.Phone == phone && c.Id != excludeId);

    public async Task<SavedAddress> AddAddressAsync(SavedAddress address)
    {
        if (address.IsDefault)
        {
            var existing = await _context.SavedAddresses
                .Where(a => a.CustomerId == address.CustomerId && a.IsDefault)
                .ToListAsync();
            foreach (var a in existing) a.IsDefault = false;
        }
        _context.SavedAddresses.Add(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task DeleteAddressAsync(SavedAddress address)
    {
        _context.SavedAddresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}
