using LinkShield.Application.DTOs.Auth;

namespace LinkShield.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);

    /// <summary>Always succeeds from the caller's point of view — never reveals whether the
    /// email exists (spec section 21: forgot-password must not leak account existence).</summary>
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);

    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    Task VerifyEmailAsync(string token, CancellationToken ct = default);
}
