using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FoodOrder.Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace FoodOrder.Infrastructure.Services;

/// <summary>
/// Handles the OAuth2 authorization code flow for external identity providers.
/// Credentials are read from the Settings table (UI-configurable) with appsettings.json as fallback.
/// </summary>
public class SsoService(
    IHttpClientFactory httpClientFactory,
    ISettingsRepository settingsRepository,
    IConfiguration config)
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // ── URL builders (async — must load DB credentials) ───────────────────────

    public async Task<string> BuildAuthorizationUrlAsync(string provider, string callbackUrl, string state)
    {
        var s = await settingsRepository.GetSettingsAsync();
        return provider.ToLowerInvariant() switch
        {
            "google"    => BuildGoogleUrl(callbackUrl, state,
                              s?.SsoGoogleClientId     ?? config["SsoProviders:Google:ClientId"] ?? ""),
            "microsoft" => BuildMicrosoftUrl(callbackUrl, state,
                              s?.SsoMicrosoftClientId  ?? config["SsoProviders:Microsoft:ClientId"] ?? "",
                              s?.SsoMicrosoftTenantId  ?? config["SsoProviders:Microsoft:TenantId"] ?? "common"),
            "facebook"  => BuildFacebookUrl(callbackUrl, state,
                              s?.SsoFacebookAppId      ?? config["SsoProviders:Facebook:AppId"] ?? ""),
            "github"    => BuildGitHubUrl(callbackUrl, state,
                              s?.SsoGitHubClientId     ?? config["SsoProviders:GitHub:ClientId"] ?? ""),
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
    }

    public async Task<bool> IsProviderConfiguredAsync(string provider)
    {
        var s = await settingsRepository.GetSettingsAsync();
        return provider.ToLowerInvariant() switch
        {
            "google"    => !string.IsNullOrEmpty(s?.SsoGoogleClientId    ?? config["SsoProviders:Google:ClientId"]),
            "microsoft" => !string.IsNullOrEmpty(s?.SsoMicrosoftClientId ?? config["SsoProviders:Microsoft:ClientId"]),
            "facebook"  => !string.IsNullOrEmpty(s?.SsoFacebookAppId     ?? config["SsoProviders:Facebook:AppId"]),
            "github"    => !string.IsNullOrEmpty(s?.SsoGitHubClientId    ?? config["SsoProviders:GitHub:ClientId"]),
            _ => false,
        };
    }

    private static string BuildGoogleUrl(string callbackUrl, string state, string clientId) =>
        "https://accounts.google.com/o/oauth2/v2/auth" +
        $"?client_id={E(clientId)}&redirect_uri={E(callbackUrl)}" +
        "&response_type=code" +
        $"&scope={E("openid email profile")}" +
        $"&state={E(state)}&access_type=offline&prompt=select_account";

    private static string BuildMicrosoftUrl(string callbackUrl, string state, string clientId, string tenant) =>
        $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize" +
        $"?client_id={E(clientId)}&redirect_uri={E(callbackUrl)}" +
        "&response_type=code" +
        $"&scope={E("openid email profile User.Read")}" +
        $"&state={E(state)}&prompt=select_account";

    private static string BuildFacebookUrl(string callbackUrl, string state, string appId) =>
        "https://www.facebook.com/v18.0/dialog/oauth" +
        $"?client_id={E(appId)}&redirect_uri={E(callbackUrl)}" +
        $"&scope={E("email,public_profile")}&state={E(state)}";

    private static string BuildGitHubUrl(string callbackUrl, string state, string clientId) =>
        "https://github.com/login/oauth/authorize" +
        $"?client_id={E(clientId)}&redirect_uri={E(callbackUrl)}" +
        $"&scope={E("user:email")}&state={E(state)}";

    // ── Code exchange ────────────────────────────────────────────────────────

    public async Task<(string Email, string Name)> ExchangeCodeAsync(string provider, string code, string callbackUrl)
    {
        var s = await settingsRepository.GetSettingsAsync();
        return provider.ToLowerInvariant() switch
        {
            "google"    => await ExchangeGoogleAsync(code, callbackUrl,
                              s?.SsoGoogleClientId     ?? config["SsoProviders:Google:ClientId"]     ?? "",
                              s?.SsoGoogleClientSecret ?? config["SsoProviders:Google:ClientSecret"] ?? ""),
            "microsoft" => await ExchangeMicrosoftAsync(code, callbackUrl,
                              s?.SsoMicrosoftClientId     ?? config["SsoProviders:Microsoft:ClientId"]     ?? "",
                              s?.SsoMicrosoftClientSecret ?? config["SsoProviders:Microsoft:ClientSecret"] ?? "",
                              s?.SsoMicrosoftTenantId     ?? config["SsoProviders:Microsoft:TenantId"]     ?? "common"),
            "facebook"  => await ExchangeFacebookAsync(code, callbackUrl,
                              s?.SsoFacebookAppId     ?? config["SsoProviders:Facebook:AppId"]     ?? "",
                              s?.SsoFacebookAppSecret ?? config["SsoProviders:Facebook:AppSecret"] ?? ""),
            "github"    => await ExchangeGitHubAsync(code, callbackUrl,
                              s?.SsoGitHubClientId     ?? config["SsoProviders:GitHub:ClientId"]     ?? "",
                              s?.SsoGitHubClientSecret ?? config["SsoProviders:GitHub:ClientSecret"] ?? ""),
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
    }

    // ── Provider-specific exchange implementations ────────────────────────────

    private async Task<(string Email, string Name)> ExchangeGoogleAsync(
        string code, string callbackUrl, string clientId, string clientSecret)
    {
        var http  = httpClientFactory.CreateClient("sso");
        var token = await PostTokenAsync(http, "https://oauth2.googleapis.com/token",
            new() { ["code"] = code, ["client_id"] = clientId, ["client_secret"] = clientSecret,
                    ["redirect_uri"] = callbackUrl, ["grant_type"] = "authorization_code" });

        var profile = await GetJsonAsync(http, "https://www.googleapis.com/oauth2/v3/userinfo",
            token.GetProperty("access_token").GetString()!);
        return (profile.GetProperty("email").GetString() ?? "", TryGet(profile, "name"));
    }

    private async Task<(string Email, string Name)> ExchangeMicrosoftAsync(
        string code, string callbackUrl, string clientId, string clientSecret, string tenant)
    {
        var http  = httpClientFactory.CreateClient("sso");
        var token = await PostTokenAsync(http,
            $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token",
            new() { ["code"] = code, ["client_id"] = clientId, ["client_secret"] = clientSecret,
                    ["redirect_uri"] = callbackUrl, ["grant_type"] = "authorization_code",
                    ["scope"] = "openid email profile User.Read" });

        var profile = await GetJsonAsync(http,
            "https://graph.microsoft.com/v1.0/me?$select=mail,userPrincipalName,displayName",
            token.GetProperty("access_token").GetString()!);

        var email = "";
        if (profile.TryGetProperty("mail", out var mail) && mail.ValueKind != JsonValueKind.Null)
            email = mail.GetString() ?? "";
        if (string.IsNullOrEmpty(email) && profile.TryGetProperty("userPrincipalName", out var upn))
            email = upn.GetString() ?? "";
        return (email, TryGet(profile, "displayName"));
    }

    private async Task<(string Email, string Name)> ExchangeFacebookAsync(
        string code, string callbackUrl, string appId, string appSecret)
    {
        var http        = httpClientFactory.CreateClient("sso");
        var tokenUrl    = $"https://graph.facebook.com/v18.0/oauth/access_token" +
                          $"?client_id={E(appId)}&client_secret={E(appSecret)}" +
                          $"&code={E(code)}&redirect_uri={E(callbackUrl)}";
        var tokenResp   = await http.GetAsync(tokenUrl);
        tokenResp.EnsureSuccessStatusCode();
        var token       = await tokenResp.Content.ReadFromJsonAsync<JsonElement>(_json);
        var accessToken = token.GetProperty("access_token").GetString()!;

        var profileResp = await http.GetAsync(
            $"https://graph.facebook.com/me?fields=id,name,email&access_token={E(accessToken)}");
        profileResp.EnsureSuccessStatusCode();
        var profile = await profileResp.Content.ReadFromJsonAsync<JsonElement>(_json);
        return (TryGet(profile, "email"), TryGet(profile, "name"));
    }

    private async Task<(string Email, string Name)> ExchangeGitHubAsync(
        string code, string callbackUrl, string clientId, string clientSecret)
    {
        var http    = httpClientFactory.CreateClient("sso");
        var tokenReq = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        tokenReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        tokenReq.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            { ["client_id"] = clientId, ["client_secret"] = clientSecret,
              ["code"] = code, ["redirect_uri"] = callbackUrl });
        var tokenResp   = await http.SendAsync(tokenReq);
        tokenResp.EnsureSuccessStatusCode();
        var token       = await tokenResp.Content.ReadFromJsonAsync<JsonElement>(_json);
        var accessToken = token.GetProperty("access_token").GetString()!;

        var profileReq = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        profileReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        profileReq.Headers.UserAgent.ParseAdd("FoodOrderPOS/1.0");
        var profileResp = await http.SendAsync(profileReq);
        profileResp.EnsureSuccessStatusCode();
        var profile = await profileResp.Content.ReadFromJsonAsync<JsonElement>(_json);

        var email = TryGet(profile, "email");

        // GitHub may hide email — fall back to /user/emails
        if (string.IsNullOrEmpty(email))
        {
            var emailsReq = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            emailsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            emailsReq.Headers.UserAgent.ParseAdd("FoodOrderPOS/1.0");
            var emailsResp = await http.SendAsync(emailsReq);
            if (emailsResp.IsSuccessStatusCode)
            {
                var emails = await emailsResp.Content.ReadFromJsonAsync<JsonElement>(_json);
                foreach (var entry in emails.EnumerateArray())
                {
                    if (entry.TryGetProperty("primary",  out var p) && p.GetBoolean() &&
                        entry.TryGetProperty("verified", out var v) && v.GetBoolean() &&
                        entry.TryGetProperty("email",    out var e))
                    { email = e.GetString() ?? ""; break; }
                }
            }
        }
        return (email, TryGet(profile, "name"));
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    private static async Task<JsonElement> PostTokenAsync(HttpClient http, string url,
        Dictionary<string, string> fields)
    {
        var resp = await http.PostAsync(url, new FormUrlEncodedContent(fields));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
    }

    private static async Task<JsonElement> GetJsonAsync(HttpClient http, string url, string bearerToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
    }

    private static string TryGet(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? "" : "";

    private static string E(string s) => Uri.EscapeDataString(s);
}
