using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo) => _repo = repo;

    public async Task<List<CustomerDto>> GetAllAsync(int orgId)
    {
        var customers = await _repo.GetAllByOrgAsync(orgId);
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _repo.GetByIdAsync(id);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(int orgId, CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            OrganizationId  = orgId,
            Name            = dto.Name,
            Phone           = dto.Phone,
            Email           = dto.Email,
            Address         = dto.Address,
            MembershipLevel = dto.MembershipLevel,
            LoyaltyPoints   = 0,
            WalletBalance   = 0,
            IsActive        = true
        };
        _repo.Add(customer);
        await _repo.SaveChangesAsync();
        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found");

        customer.Name            = dto.Name;
        customer.Phone           = dto.Phone;
        customer.Email           = dto.Email;
        customer.Address         = dto.Address;
        customer.MembershipLevel = dto.MembershipLevel;
        customer.LoyaltyPoints   = dto.LoyaltyPoints;
        customer.WalletBalance   = dto.WalletBalance;
        customer.IsActive        = dto.IsActive;
        customer.UpdatedDate     = DateTime.UtcNow;

        _repo.Update(customer);
        await _repo.SaveChangesAsync();
        return MapToDto(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found");
        _repo.Remove(customer);
        await _repo.SaveChangesAsync();
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id              = c.Id,
        Name            = c.Name,
        Phone           = c.Phone,
        Email           = c.Email,
        Address         = c.Address,
        MembershipLevel = c.MembershipLevel,
        LoyaltyPoints   = c.LoyaltyPoints,
        WalletBalance   = c.WalletBalance,
        IsActive        = c.IsActive
    };
}
