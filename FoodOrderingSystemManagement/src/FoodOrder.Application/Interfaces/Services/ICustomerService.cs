using FoodOrder.Application.DTOs.Customer;

namespace FoodOrder.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto?> GetByPhoneAsync(string phone);
    Task<IReadOnlyList<PointsTransactionDto>> GetTransactionsAsync(int customerId);
    Task<IReadOnlyList<CustomerOrderSummaryDto>> GetOrdersAsync(int customerId);
    Task<IReadOnlyList<SavedAddressDto>> GetAddressesAsync(int customerId);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest dto);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerRequest dto);
    Task<CustomerDto?> AdjustPointsAsync(int id, AdjustPointsRequest dto);
    Task DeleteAsync(int id);
    Task<SavedAddressDto?> AddAddressAsync(int customerId, AddSavedAddressRequest dto);
    Task DeleteAddressAsync(int customerId, int addressId);
}
