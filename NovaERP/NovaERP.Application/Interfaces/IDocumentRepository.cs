using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllByOrgAsync(int orgId, string? entityType, int? entityId);
    Task<Document?> GetByIdAsync(int orgId, int id);
    void Add(Document document);
    void Remove(Document document);
    Task SaveChangesAsync();
}
