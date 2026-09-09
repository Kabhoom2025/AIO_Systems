using FoodOrder.Application.DTOs.Auth;

namespace FoodOrder.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> SsoLoginAsync(string email, string providerName);
    Task<string> ForgotPasswordAsync(string email, string resetBaseUrl);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
