using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllAsync(int orgId, string? entityType, int? entityId);

    Task<DocumentDto> UploadAsync(int orgId, int uploadedByUserId, Stream content, string fileName,
        string contentType, long sizeBytes, string? entityType, int? entityId);

    Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(int orgId, int id);

    Task DeleteAsync(int orgId, int id);
}
