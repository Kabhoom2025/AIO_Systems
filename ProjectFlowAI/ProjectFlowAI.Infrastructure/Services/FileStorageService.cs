using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>Local-disk implementation of IFileStorageService. BasePath is supplied by the host
/// (ProjectFlowAI.API/Program.cs wires it to {ContentRoot}/App_Data/attachments) so Infrastructure
/// doesn't need to know about ASP.NET Core hosting types.</summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<StoredFile> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var safeExtension = Path.GetExtension(fileName);
        var relativePath = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = Path.Combine(_basePath, relativePath);

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return new StoredFile(relativePath, fileStream.Length);
    }

    public Stream OpenRead(string relativePath) =>
        new FileStream(Path.Combine(_basePath, relativePath), FileMode.Open, FileAccess.Read);

    public void Delete(string relativePath)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".zip" => "application/zip",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
