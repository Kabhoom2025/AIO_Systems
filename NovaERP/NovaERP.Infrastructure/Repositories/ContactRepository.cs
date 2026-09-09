using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly NovaErpDbContext _ctx;

    public ContactRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Contact>> GetAllByOrgAsync(int orgId) =>
        _ctx.Contacts
            .Include(c => c.Owner)
            .Include(c => c.Account)
            .Where(c => c.OrganizationId == orgId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

    public Task<Contact?> GetByIdAsync(int orgId, int id) =>
        _ctx.Contacts
            .Include(c => c.Owner)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public Task<Contact?> GetByUserIdAsync(int orgId, int userId) =>
        _ctx.Contacts
            .Include(c => c.Owner)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.OrganizationId == orgId);

    public void Add(Contact contact)    => _ctx.Contacts.Add(contact);
    public void Update(Contact contact) => _ctx.Contacts.Update(contact);
    public void Remove(Contact contact) => _ctx.Contacts.Remove(contact);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
