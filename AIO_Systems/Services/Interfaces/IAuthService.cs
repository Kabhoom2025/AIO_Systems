using AIO_Systems.DTOs.Auth;

namespace AIO_Systems.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
