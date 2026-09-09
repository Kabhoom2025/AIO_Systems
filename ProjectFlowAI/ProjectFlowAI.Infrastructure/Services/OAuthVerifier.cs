using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

public class OAuthVerifier : IOAuthVerifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public OAuthVerifier(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<OAuthUserInfo> VerifyGoogleIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(nameof(OAuthVerifier));
        var response = await client.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new OAuthVerificationException("Google ID token verification failed.");

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenInfo>(cancellationToken: cancellationToken)
            ?? throw new OAuthVerificationException("Google ID token verification returned no payload.");

        var expectedClientId = _config["OAuth:GoogleClientId"];
        if (!string.IsNullOrWhiteSpace(expectedClientId) && payload.aud != expectedClientId)
            throw new OAuthVerificationException("Google ID token audience mismatch.");

        if (string.IsNullOrWhiteSpace(payload.email))
            throw new OAuthVerificationException("Google ID token did not include an email address.");

        return new OAuthUserInfo(payload.email, payload.name ?? payload.email, payload.sub ?? payload.email);
    }

    public async Task<OAuthUserInfo> VerifyMicrosoftAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(nameof(OAuthVerifier));
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new OAuthVerificationException("Microsoft Graph token verification failed.");

        var payload = await response.Content.ReadFromJsonAsync<MicrosoftGraphUser>(cancellationToken: cancellationToken)
            ?? throw new OAuthVerificationException("Microsoft Graph verification returned no payload.");

        var email = payload.mail ?? payload.userPrincipalName;
        if (string.IsNullOrWhiteSpace(email))
            throw new OAuthVerificationException("Microsoft Graph profile did not include an email address.");

        return new OAuthUserInfo(email, payload.displayName ?? email, payload.id ?? email);
    }

    // Minimal DTOs for the two upstream JSON payloads — deliberately not exposed outside this class.
    private sealed class GoogleTokenInfo
    {
        public string? aud { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }
        public string? sub { get; set; }
    }

    private sealed class MicrosoftGraphUser
    {
        public string? id { get; set; }
        public string? displayName { get; set; }
        public string? mail { get; set; }
        public string? userPrincipalName { get; set; }
    }
}
