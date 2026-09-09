using FoodOrder.Application.DTOs.Backup;

namespace FoodOrder.Application.Interfaces.Services;

public interface IBackupService
{
    Task<IEnumerable<BackupDto>> ListAsync();
    Task<BackupDto> CreateAsync();
    Task<(byte[] Data, string FileName, string ContentType)> DownloadAsync(string fileName);
    Task RestoreAsync(string fileName);
    Task DeleteAsync(string fileName);
}
