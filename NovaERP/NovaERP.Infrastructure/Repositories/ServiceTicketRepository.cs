using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ServiceTicketRepository : IServiceTicketRepository
{
    private readonly NovaErpDbContext _ctx;

    public ServiceTicketRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<ServiceTicket> Query() =>
        _ctx.ServiceTickets.Include(t => t.Category).Include(t => t.Requester).Include(t => t.AssignedTo);

    public Task<List<ServiceTicket>> GetAllByOrgAsync(int orgId) =>
        Query().Where(t => t.OrganizationId == orgId).OrderBy(t => t.TicketNumber).ToListAsync();

    public Task<ServiceTicket?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == orgId);

    public Task<bool> TicketNumberExistsAsync(int orgId, string ticketNumber) =>
        _ctx.ServiceTickets.AnyAsync(t => t.OrganizationId == orgId && t.TicketNumber == ticketNumber);

    public void Add(ServiceTicket ticket)    => _ctx.ServiceTickets.Add(ticket);
    public void Update(ServiceTicket ticket) => _ctx.ServiceTickets.Update(ticket);
    public void Remove(ServiceTicket ticket) => _ctx.ServiceTickets.Remove(ticket);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
