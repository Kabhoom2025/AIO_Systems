namespace Chatbot.Application.Auth;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Role);

public record AuthResponse(UserDto User, string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
