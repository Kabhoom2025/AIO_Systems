using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class RfqRequestRepository : IRfqRequestRepository
{
    private readonly NovaErpDbContext _ctx;

    public RfqRequestRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<RfqRequest> Query() =>
        _ctx.RfqRequests
            .Include(r => r.Owner)
            .Include(r => r.WinningVendor)
            .Include(r => r.Items)
            .Include(r => r.Quotes).ThenInclude(q => q.Vendor);

    public Task<List<RfqRequest>> GetAllByOrgAsync(int orgId) =>
        Query().Where(r => r.OrganizationId == orgId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public Task<RfqRequest?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId);

    public void Add(RfqRequest rfq)    => _ctx.RfqRequests.Add(rfq);
    public void Update(RfqRequest rfq) => _ctx.RfqRequests.Update(rfq);
    public void Remove(RfqRequest rfq) => _ctx.RfqRequests.Remove(rfq);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
