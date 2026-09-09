using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NovaERP.Application.Interfaces;

namespace NovaERP.Infrastructure.Services;

/// <summary>Disk-backed IFileStorageService. The root folder is config-driven
/// (FileStorage:RootPath, relative to the API's content root) so this can be swapped for a
/// blob-storage implementation later without touching IDocumentService or its callers.</summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredRoot = configuration["FileStorage:RootPath"] ?? "App_Data/documents";
        _rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string organizationCode)
    {
        var orgFolder = Path.Combine(_rootPath, organizationCode);
        Directory.CreateDirectory(orgFolder);

        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(orgFolder, safeFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream);
        }

        // Stored as a relative, forward-slash path so it's portable across OSes.
        return Path.Combine(organizationCode, safeFileName).Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
