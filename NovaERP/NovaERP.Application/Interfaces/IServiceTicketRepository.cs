using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IServiceTicketRepository
{
    Task<List<ServiceTicket>> GetAllByOrgAsync(int orgId);
    Task<ServiceTicket?> GetByIdAsync(int orgId, int id);
    Task<bool> TicketNumberExistsAsync(int orgId, string ticketNumber);

    void Add(ServiceTicket ticket);
    void Update(ServiceTicket ticket);
    void Remove(ServiceTicket ticket);
    Task SaveChangesAsync();
}
