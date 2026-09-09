namespace Platform.Application.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Email,
    string DisplayName);
