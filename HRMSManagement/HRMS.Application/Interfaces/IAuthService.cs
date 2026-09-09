using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto, string? ipAddress, string? userAgent);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    /// <summary>Returns the reset token (in production this would be emailed, never returned).</summary>
    Task<string?> ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task<List<LoginHistoryDto>> GetLoginHistoryAsync(int orgId, int take = 100);
}
