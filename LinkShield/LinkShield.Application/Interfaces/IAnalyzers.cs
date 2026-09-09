using LinkShield.Application.DTOs.Analysis;

namespace LinkShield.Application.Interfaces;

/// <summary>Queries RDAP for the registrable domain. Makes a real outbound HTTP call — routed
/// through the SSRF-safe client. Never throws on lookup failure: reports
/// RdapLookupSucceeded=false + RdapError instead, since a scan should still complete with
/// partial information when RDAP is unavailable for a given TLD.</summary>
public interface IDomainAnalyzer
{
    Task<DomainAnalysisResultDto> AnalyzeAsync(string registeredDomain, CancellationToken ct = default);
}

/// <summary>Resolves DNS records for a domain. Never throws on resolution failure: reports
/// Resolves=false + LookupError instead.</summary>
public interface IDnsAnalyzer
{
    Task<DnsAnalysisResultDto> AnalyzeAsync(string domain, CancellationToken ct = default);
}

/// <summary>Opens a real TLS connection (through the SSRF-safe connector) to inspect the
/// certificate actually presented for the host. Never throws on handshake/connection failure:
/// reports HasCertificate=false + ChainValidationError instead.</summary>
public interface ISslAnalyzer
{
    Task<SslAnalysisResultDto> AnalyzeAsync(string host, CancellationToken ct = default);
}

/// <summary>Follows HTTP redirects manually (no automatic following) so every hop can be
/// SSRF-validated and recorded before being followed. Never throws on failure: reports
/// the Error field and whatever hops were captured before the failure.</summary>
public interface IRedirectAnalyzer
{
    Task<RedirectAnalysisResultDto> AnalyzeAsync(string startUrl, CancellationToken ct = default);
}
