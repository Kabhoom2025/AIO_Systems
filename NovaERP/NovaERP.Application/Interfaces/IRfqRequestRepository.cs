using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IRfqRequestRepository
{
    Task<List<RfqRequest>> GetAllByOrgAsync(int orgId);
    Task<RfqRequest?> GetByIdAsync(int orgId, int id);
    void Add(RfqRequest rfq);
    void Update(RfqRequest rfq);
    void Remove(RfqRequest rfq);
    Task SaveChangesAsync();
}
