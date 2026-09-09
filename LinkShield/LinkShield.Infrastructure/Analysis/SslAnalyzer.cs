using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.Analysis;

/// <summary>Opens a real TLS connection to inspect the certificate actually presented — HTTPS
/// must never be treated as proof of legitimacy (spec section 8), only reported as a signal.
/// The connection itself goes through ISsrfSafeConnector exactly like every other
/// user-influenced-host connection this platform makes.</summary>
public class SslAnalyzer : ISslAnalyzer
{
    private readonly ISsrfSafeConnector _connector;
    private readonly int _timeoutMs;

    public SslAnalyzer(ISsrfSafeConnector connector, IConfiguration configuration)
    {
        _connector = connector;
        _timeoutMs = int.TryParse(configuration["SsrfProtection:RequestTimeoutMs"], out var t) ? t : 5000;
    }

    public async Task<SslAnalysisResultDto> AnalyzeAsync(string host, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeoutMs);

        Socket? socket = null;
        SslStream? sslStream = null;
        try
        {
            socket = await _connector.ConnectAsync(host, 443, cts.Token);
            var networkStream = new NetworkStream(socket, ownsSocket: true);

            X509Certificate2? presentedCert = null;
            SslPolicyErrors capturedErrors = SslPolicyErrors.None;

            sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, (_, cert, _, errors) =>
            {
                presentedCert = cert is null ? null : new X509Certificate2(cert);
                capturedErrors = errors;
                return true; // always complete the handshake so we can inspect the cert; validity is reported, not enforced here
            });

            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = host,
                EnabledSslProtocols = SslProtocols.None // let the OS negotiate the best available
            }, cts.Token);

            if (presentedCert is null)
                return NoCertificate("Server completed the TLS handshake but presented no certificate.");

            var sanNames = presentedCert.Extensions
                .OfType<X509SubjectAlternativeNameExtension>()
                .FirstOrDefault()
                ?.EnumerateDnsNames()
                .ToList() ?? [];

            var isExpired = DateTime.UtcNow > presentedCert.NotAfter.ToUniversalTime();
            var isSelfSigned = presentedCert.Subject == presentedCert.Issuer;
            var domainMatches = MatchesHost(host, sanNames, presentedCert.GetNameInfo(X509NameType.SimpleName, false));
            var isValid = capturedErrors == SslPolicyErrors.None && !isExpired;

            var score = 0;
            if (isExpired) score += 25;
            if (isSelfSigned) score += 20;
            if (!domainMatches) score += 20;
            if (capturedErrors != SslPolicyErrors.None) score += 15;

            return new SslAnalysisResultDto(
                HasCertificate: true,
                IsValid: isValid,
                Issuer: presentedCert.Issuer,
                Subject: presentedCert.Subject,
                SubjectAlternativeNames: sanNames,
                ValidFromUtc: presentedCert.NotBefore.ToUniversalTime(),
                ValidToUtc: presentedCert.NotAfter.ToUniversalTime(),
                IsExpired: isExpired,
                IsSelfSigned: isSelfSigned,
                DomainMatchesCertificate: domainMatches,
                TlsVersion: sslStream.SslProtocol.ToString(),
                ChainValidationError: capturedErrors == SslPolicyErrors.None ? null : capturedErrors.ToString(),
                SslAnalysisScore: Math.Min(score, 100));
        }
        catch (SsrfBlockedException ex)
        {
            return NoCertificate($"Connection blocked: {ex.Message}");
        }
        catch (AuthenticationException ex)
        {
            return NoCertificate($"TLS handshake failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return NoCertificate("Connection or handshake timed out.");
        }
        catch (Exception ex)
        {
            return NoCertificate(ex.Message);
        }
        finally
        {
            sslStream?.Dispose();
            socket?.Dispose();
        }
    }

    private static bool MatchesHost(string host, IReadOnlyList<string> sanNames, string? commonName)
    {
        bool Matches(string pattern) =>
            string.Equals(pattern, host, StringComparison.OrdinalIgnoreCase) ||
            (pattern.StartsWith("*.") && host.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase));

        if (sanNames.Any(Matches)) return true;
        return commonName is not null && Matches(commonName);
    }

    private static SslAnalysisResultDto NoCertificate(string error) => new(
        HasCertificate: false,
        IsValid: false,
        Issuer: null,
        Subject: null,
        SubjectAlternativeNames: [],
        ValidFromUtc: null,
        ValidToUtc: null,
        IsExpired: false,
        IsSelfSigned: false,
        DomainMatchesCertificate: false,
        TlsVersion: null,
        ChainValidationError: error,
        SslAnalysisScore: 0);
}
