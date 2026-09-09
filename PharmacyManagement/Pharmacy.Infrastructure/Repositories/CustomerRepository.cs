using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly PharmacyDbContext _ctx;

    public CustomerRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Customer>> GetAllByOrgAsync(int orgId) =>
        _ctx.Customers
            .Where(c => c.OrganizationId == orgId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public Task<Customer?> GetByIdAsync(int id) =>
        _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);

    public void Add(Customer customer)    => _ctx.Customers.Add(customer);
    public void Update(Customer customer) => _ctx.Customers.Update(customer);
    public void Remove(Customer customer) => _ctx.Customers.Remove(customer);
    public Task SaveChangesAsync()        => _ctx.SaveChangesAsync();
}
