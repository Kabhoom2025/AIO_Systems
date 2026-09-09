using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class TicketCategoryRepository : ITicketCategoryRepository
{
    private readonly NovaErpDbContext _ctx;

    public TicketCategoryRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<TicketCategory>> GetAllByOrgAsync(int orgId) =>
        _ctx.TicketCategories.Where(c => c.OrganizationId == orgId).OrderBy(c => c.Code).ToListAsync();

    public Task<TicketCategory?> GetByIdAsync(int orgId, int id) =>
        _ctx.TicketCategories.FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.TicketCategories.AnyAsync(c => c.OrganizationId == orgId && c.Code == code);

    public void Add(TicketCategory category)    => _ctx.TicketCategories.Add(category);
    public void Update(TicketCategory category) => _ctx.TicketCategories.Update(category);
    public void Remove(TicketCategory category) => _ctx.TicketCategories.Remove(category);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
