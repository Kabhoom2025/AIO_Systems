using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly NovaErpDbContext _ctx;

    public DocumentRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Document>> GetAllByOrgAsync(int orgId, string? entityType, int? entityId)
    {
        var query = _ctx.Documents.Where(d => d.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(d => d.EntityType == entityType);
        if (entityId.HasValue)
            query = query.Where(d => d.EntityId == entityId.Value);

        return query.OrderByDescending(d => d.UploadedDate).ToListAsync();
    }

    public Task<Document?> GetByIdAsync(int orgId, int id) =>
        _ctx.Documents.FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

    public void Add(Document document)    => _ctx.Documents.Add(document);
    public void Remove(Document document) => _ctx.Documents.Remove(document);
    public Task SaveChangesAsync()        => _ctx.SaveChangesAsync();
}
