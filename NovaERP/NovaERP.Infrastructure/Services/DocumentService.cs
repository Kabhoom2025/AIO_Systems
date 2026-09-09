using AutoMapper;
using Microsoft.Extensions.Configuration;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Services;

/// <summary>Lives in Infrastructure (not Application/Services, unlike Branch/Currency/etc.)
/// because it needs IConfiguration for the size limit and IFileStorageService for the actual
/// bytes — mirrors how NotificationService also sits in Infrastructure.</summary>
public class DocumentService : IDocumentService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // docx
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",       // xlsx
        "text/csv"
    };

    private const long DefaultMaxSizeBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IDocumentRepository _repo;
    private readonly IFileStorageService _storage;
    private readonly IMapper _mapper;
    private readonly long _maxSizeBytes;

    public DocumentService(IDocumentRepository repo, IFileStorageService storage, IMapper mapper, IConfiguration configuration)
    {
        _repo = repo;
        _storage = storage;
        _mapper = mapper;
        _maxSizeBytes = long.TryParse(configuration["FileStorage:MaxSizeBytes"], out var configured)
            ? configured
            : DefaultMaxSizeBytes;
    }

    public async Task<List<DocumentDto>> GetAllAsync(int orgId, string? entityType, int? entityId)
    {
        var docs = await _repo.GetAllByOrgAsync(orgId, entityType, entityId);
        return docs.Select(_mapper.Map<DocumentDto>).ToList();
    }

    public async Task<DocumentDto> UploadAsync(int orgId, int uploadedByUserId, Stream content, string fileName,
        string contentType, long sizeBytes, string? entityType, int? entityId)
    {
        if (sizeBytes <= 0)
            throw new InvalidOperationException("Uploaded file is empty.");
        if (sizeBytes > _maxSizeBytes)
            throw new InvalidOperationException($"File exceeds the maximum allowed size of {_maxSizeBytes / (1024 * 1024)} MB.");
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException($"File type '{contentType}' is not allowed.");

        var storagePath = await _storage.SaveAsync(content, fileName, orgId.ToString());

        var document = new Document
        {
            OrganizationId   = orgId,
            FileName         = fileName,
            ContentType      = contentType,
            SizeBytes        = sizeBytes,
            StoragePath      = storagePath,
            EntityType       = entityType,
            EntityId         = entityId,
            UploadedByUserId = uploadedByUserId,
            UploadedDate     = DateTime.UtcNow
        };

        _repo.Add(document);
        await _repo.SaveChangesAsync();
        return _mapper.Map<DocumentDto>(document);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(int orgId, int id)
    {
        var document = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Document {id} not found");

        var stream = await _storage.OpenReadAsync(document.StoragePath);
        return (stream, document.FileName, document.ContentType);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var document = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Document {id} not found");

        await _storage.DeleteAsync(document.StoragePath);
        _repo.Remove(document);
        await _repo.SaveChangesAsync();
    }
}
