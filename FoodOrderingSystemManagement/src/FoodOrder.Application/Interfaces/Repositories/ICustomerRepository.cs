using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<IReadOnlyList<PointsTransaction>> GetTransactionsAsync(int customerId);
    Task<IReadOnlyList<Order>> GetOrdersAsync(int customerId);
    Task<IReadOnlyList<SavedAddress>> GetAddressesAsync(int customerId);
    Task<SavedAddress?> GetAddressByIdAsync(int addressId);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task AddTransactionAsync(PointsTransaction txn);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
    Task<SavedAddress> AddAddressAsync(SavedAddress address);
    Task DeleteAddressAsync(SavedAddress address);
}
