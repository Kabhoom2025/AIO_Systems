using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Customer;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository repo, ICurrentUserContext currentUser, IMapper mapper)
    {
        _repo   = repo;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync()
    {
        var customers = await _repo.GetAllAsync();
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c is null ? null : MapToDto(c);
    }

    public async Task<CustomerDto?> GetByPhoneAsync(string phone)
    {
        var c = await _repo.GetByPhoneAsync(phone.Trim());
        return c is null ? null : MapToDto(c);
    }

    public async Task<IReadOnlyList<PointsTransactionDto>> GetTransactionsAsync(int customerId)
    {
        var txns = await _repo.GetTransactionsAsync(customerId);
        return txns.Select(t => new PointsTransactionDto
        {
            Id          = t.Id,
            Points      = t.Points,
            Type        = t.Type,
            Description = t.Description,
            OrderId     = t.OrderId,
            CreatedDate = t.CreatedDate
        }).ToList();
    }

    public async Task<IReadOnlyList<CustomerOrderSummaryDto>> GetOrdersAsync(int customerId)
    {
        var orders = await _repo.GetOrdersAsync(customerId);
        return orders.Select(o => new CustomerOrderSummaryDto
        {
            Id             = o.Id,
            OrderNumber    = o.OrderNumber,
            OrderDate      = o.OrderDate,
            GrandTotal     = o.GrandTotal,
            Status         = o.Status.ToString(),
            PointsEarned   = o.PointsEarned,
            PointsRedeemed = o.PointsRedeemed,
        }).ToList();
    }

    public async Task<IReadOnlyList<SavedAddressDto>> GetAddressesAsync(int customerId)
    {
        var addresses = await _repo.GetAddressesAsync(customerId);
        return addresses.Select(MapAddressToDto).ToList();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest dto)
    {
        if (await _repo.PhoneExistsAsync(dto.Phone.Trim()))
            throw new InvalidOperationException($"Phone number '{dto.Phone}' is already registered.");

        var customer = new Customer
        {
            OrganizationId = TenantResolution.ResolveWriteOrganizationId(_currentUser),
            Name        = dto.Name.Trim(),
            Phone       = dto.Phone.Trim(),
            Email       = dto.Email?.Trim(),
            Birthday    = dto.Birthday.HasValue ? DateTime.SpecifyKind(dto.Birthday.Value, DateTimeKind.Utc) : null,
            Notes       = dto.Notes?.Trim(),
            IsActive    = true,
            CreatedDate = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(customer);
        return MapToDto(created);
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerRequest dto)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null) return null;

        customer.Name     = dto.Name.Trim();
        customer.Email    = dto.Email?.Trim();
        customer.Birthday = dto.Birthday.HasValue ? DateTime.SpecifyKind(dto.Birthday.Value, DateTimeKind.Utc) : null;
        customer.Notes    = dto.Notes?.Trim();
        customer.IsActive = dto.IsActive;

        await _repo.UpdateAsync(customer);
        return MapToDto(customer);
    }

    public async Task<CustomerDto?> AdjustPointsAsync(int id, AdjustPointsRequest dto)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null) return null;

        customer.LoyaltyPoints += dto.Points;
        if (customer.LoyaltyPoints < 0) customer.LoyaltyPoints = 0;

        await _repo.AddTransactionAsync(new PointsTransaction
        {
            CustomerId  = id,
            Points      = dto.Points,
            Type        = "Adjust",
            Description = dto.Description.Trim(),
            CreatedDate = DateTime.UtcNow
        });

        await _repo.UpdateAsync(customer);
        return MapToDto(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null) return;
        await _repo.DeleteAsync(customer);
    }

    public async Task<SavedAddressDto?> AddAddressAsync(int customerId, AddSavedAddressRequest dto)
    {
        var customer = await _repo.GetByIdAsync(customerId);
        if (customer is null) return null;

        var address = new SavedAddress
        {
            CustomerId  = customerId,
            Label       = dto.Label.Trim(),
            AddressLine = dto.AddressLine.Trim(),
            City        = dto.City?.Trim(),
            IsDefault   = dto.IsDefault,
            CreatedDate = DateTime.UtcNow,
        };

        var created = await _repo.AddAddressAsync(address);
        return MapAddressToDto(created);
    }

    public async Task DeleteAddressAsync(int customerId, int addressId)
    {
        var address = await _repo.GetAddressByIdAsync(addressId);
        if (address is null || address.CustomerId != customerId) return;
        await _repo.DeleteAddressAsync(address);
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id            = c.Id,
        Name          = c.Name,
        Phone         = c.Phone,
        Email         = c.Email,
        Birthday      = c.Birthday,
        Notes         = c.Notes,
        IsActive      = c.IsActive,
        LoyaltyPoints = c.LoyaltyPoints,
        TotalSpend    = c.TotalSpend,
        TotalVisits   = c.TotalVisits,
        Tier          = c.Tier,
        CreatedDate   = c.CreatedDate
    };

    private static SavedAddressDto MapAddressToDto(SavedAddress a) => new()
    {
        Id          = a.Id,
        Label       = a.Label,
        AddressLine = a.AddressLine,
        City        = a.City,
        IsDefault   = a.IsDefault,
    };
}
