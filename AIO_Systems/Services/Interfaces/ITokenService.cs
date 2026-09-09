using AIO_Systems.Domain.Entities;
using AIO_Systems.DTOs.Auth;

namespace AIO_Systems.Services.Interfaces;

/// <summary>
/// Abstracts JWT generation so the auth service is not coupled to System.IdentityModel.
/// Returns a TokenResult containing both the signed token string and its expiry.
/// </summary>
public interface ITokenService
{
    TokenResult GenerateToken(User user);
}
