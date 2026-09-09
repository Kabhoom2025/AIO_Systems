using DnsClient;
using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.Analysis;

public class DnsAnalyzer : IDnsAnalyzer
{
    private static readonly string[] SuspiciousNameserverProviders =
    [
        "afraid.org", "dynu.com", "no-ip.com", "noip.com", "duckdns.org", "freedns.afraid.org"
    ];

    private readonly LookupClient _lookupClient;

    public DnsAnalyzer(IConfiguration configuration)
    {
        var timeoutMs = int.TryParse(configuration["SsrfProtection:RequestTimeoutMs"], out var t) ? t : 5000;
        _lookupClient = new LookupClient(new LookupClientOptions
        {
            Timeout = TimeSpan.FromMilliseconds(timeoutMs),
            UseCache = false,
            ThrowDnsErrors = false
        });
    }

    public async Task<DnsAnalysisResultDto> AnalyzeAsync(string domain, CancellationToken ct = default)
    {
        try
        {
            var aTask = _lookupClient.QueryAsync(domain, QueryType.A, cancellationToken: ct);
            var aaaaTask = _lookupClient.QueryAsync(domain, QueryType.AAAA, cancellationToken: ct);
            var mxTask = _lookupClient.QueryAsync(domain, QueryType.MX, cancellationToken: ct);
            var nsTask = _lookupClient.QueryAsync(domain, QueryType.NS, cancellationToken: ct);
            var cnameTask = _lookupClient.QueryAsync(domain, QueryType.CNAME, cancellationToken: ct);
            var txtTask = _lookupClient.QueryAsync(domain, QueryType.TXT, cancellationToken: ct);

            await Task.WhenAll(aTask, aaaaTask, mxTask, nsTask, cnameTask, txtTask);

            var aRecords = aTask.Result.Answers.ARecords().Select(r => r.Address.ToString()).ToList();
            var aaaaRecords = aaaaTask.Result.Answers.AaaaRecords().Select(r => r.Address.ToString()).ToList();
            var mxRecords = mxTask.Result.Answers.MxRecords().Select(r => r.Exchange.Value.TrimEnd('.')).ToList();
            var nsRecords = nsTask.Result.Answers.NsRecords().Select(r => r.NSDName.Value.TrimEnd('.')).ToList();
            var cnameRecords = cnameTask.Result.Answers.CnameRecords().Select(r => r.CanonicalName.Value.TrimEnd('.')).ToList();
            var txtRecords = txtTask.Result.Answers.TxtRecords().SelectMany(r => r.Text).ToList();

            var resolves = aRecords.Count > 0 || aaaaRecords.Count > 0 || cnameRecords.Count > 0;
            var hasMailConfiguration = mxRecords.Count > 0;
            var hasSuspiciousNameservers = nsRecords.Any(ns =>
                SuspiciousNameserverProviders.Any(p => ns.EndsWith(p, StringComparison.OrdinalIgnoreCase)));

            var score = 0;
            if (!resolves) score += 10;
            if (hasSuspiciousNameservers) score += 20;
            if (!hasMailConfiguration) score += 5;

            return new DnsAnalysisResultDto(
                resolves, aRecords, aaaaRecords, mxRecords, nsRecords, cnameRecords, txtRecords,
                hasMailConfiguration, hasSuspiciousNameservers, LookupError: null, Math.Min(score, 100));
        }
        catch (Exception ex)
        {
            return new DnsAnalysisResultDto(
                Resolves: false, [], [], [], [], [], [],
                HasMailConfiguration: false, HasSuspiciousNameservers: false,
                LookupError: ex.Message, DnsAnalysisScore: 0);
        }
    }
}
