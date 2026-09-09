using FoodOrder.Application.DTOs.Auth;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Services;

/// <summary>
/// Abstracts JWT generation so the auth service is not coupled to System.IdentityModel.
/// Returns a TokenResult containing both the signed token string and its expiry.
/// </summary>
public interface ITokenService
{
    TokenResult GenerateToken(User user);
}
