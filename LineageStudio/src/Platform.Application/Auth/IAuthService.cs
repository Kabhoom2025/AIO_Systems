namespace Platform.Application.Auth;

public interface IAuthService
{
    /// <summary>Returns null on invalid credentials - never throws for a bad login, so a wrong
    /// password can't be distinguished from "no such user" by timing or exception shape.</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
