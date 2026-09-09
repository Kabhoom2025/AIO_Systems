using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllByOrgAsync(int orgId);
    Task<Customer?> GetByIdAsync(int id);
    void Add(Customer customer);
    void Update(Customer customer);
    void Remove(Customer customer);
    Task SaveChangesAsync();
}
