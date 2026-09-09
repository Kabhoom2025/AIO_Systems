namespace NovaERP.Application.Interfaces;

/// <summary>Abstraction over "where document bytes physically live", so the on-disk
/// implementation used in this phase can be swapped for blob storage later without
/// touching IDocumentService or DocumentController.</summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string organizationCode);
    Task<Stream> OpenReadAsync(string storagePath);
    Task DeleteAsync(string storagePath);
}
