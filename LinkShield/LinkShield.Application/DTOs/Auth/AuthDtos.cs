namespace LinkShield.Application.DTOs.Auth;

public record RegisterRequestDto(string Email, string Password, string FirstName, string LastName);

public record LoginRequestDto(string Email, string Password);

public record RefreshRequestDto(string RefreshToken);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Token, string NewPassword);

public record VerifyEmailRequestDto(string Token);

public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
