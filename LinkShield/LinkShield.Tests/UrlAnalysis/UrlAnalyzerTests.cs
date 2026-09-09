using LinkShield.Application.Services;
using Xunit;

namespace LinkShield.Tests.UrlAnalysis;

public class UrlAnalyzerTests
{
    private readonly UrlAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_PlainHttpsUrl_FlagsNothingSuspicious()
    {
        var result = _analyzer.Analyze("https://www.example.com/about");

        Assert.True(result.IsHttps);
        Assert.False(result.IsIpAddressHost);
        Assert.False(result.HasPunycode);
        Assert.False(result.HasSuspiciousTld);
        Assert.False(result.IsShortenedUrl);
        Assert.Empty(result.SuspiciousKeywords);
        Assert.Equal(0, result.UrlAnalysisScore);
    }

    [Fact]
    public void Analyze_IpAddressHost_IsFlagged()
    {
        var result = _analyzer.Analyze("http://192.168.1.5/login");

        Assert.True(result.IsIpAddressHost);
        Assert.True(result.UrlAnalysisScore > 0);
    }

    [Fact]
    public void Analyze_PunycodeHost_IsFlagged()
    {
        var result = _analyzer.Analyze("https://xn--pypal-4ve.com/verify");

        Assert.True(result.HasPunycode);
    }

    [Fact]
    public void Analyze_HomoglyphHost_IsFlagged()
    {
        // Cyrillic "а" (U+0430) mixed with Latin letters in the same label.
        var result = _analyzer.Analyze("https://pаypal.com/login");

        Assert.True(result.HasHomoglyphs);
    }

    [Fact]
    public void Analyze_KnownShortenerHost_IsFlagged()
    {
        var result = _analyzer.Analyze("https://bit.ly/3xample");

        Assert.True(result.IsShortenedUrl);
    }

    [Fact]
    public void Analyze_SuspiciousTld_IsFlagged()
    {
        var result = _analyzer.Analyze("http://free-prize.tk/claim");

        Assert.True(result.HasSuspiciousTld);
    }

    [Fact]
    public void Analyze_AuthKeywordInPath_IsFlagged()
    {
        var result = _analyzer.Analyze("https://example.com/account/verify");

        Assert.Contains("verify", result.SuspiciousKeywords);
        Assert.Contains("account", result.SuspiciousKeywords);
    }

    [Fact]
    public void Analyze_RedirectParameter_IsFlagged()
    {
        var result = _analyzer.Analyze("https://example.com/go?redirect=http://evil.example");

        Assert.True(result.HasRedirectParameter);
        Assert.Contains("redirect", result.SuspiciousParameters);
    }

    [Fact]
    public void Analyze_SuspiciousFileExtension_IsFlagged()
    {
        var result = _analyzer.Analyze("http://example.com/downloads/invoice.exe");

        Assert.True(result.HasSuspiciousFileExtension);
    }

    [Fact]
    public void Analyze_DoubleEncodedUrl_IsFlagged()
    {
        var result = _analyzer.Analyze("http://example.com/redirect?next=%2570ath");

        Assert.True(result.HasDoubleEncoding);
    }

    [Fact]
    public void Analyze_SubdomainCount_CountsCorrectly()
    {
        var result = _analyzer.Analyze("https://a.b.c.example.com/");

        Assert.Equal(3, result.SubdomainCount);
    }

    [Fact]
    public void Analyze_MultiPartTld_ComputesRegisteredDomainCorrectly()
    {
        var result = _analyzer.Analyze("https://mail.example.co.uk/");

        Assert.Equal(1, result.SubdomainCount);
    }

    [Fact]
    public void Analyze_NormalizesHostToLowercaseAndStripsDefaultPort()
    {
        var result = _analyzer.Analyze("HTTPS://Example.COM:443/Path");

        Assert.Equal("https://example.com/Path", result.NormalizedUrl);
    }

    [Fact]
    public void Analyze_MultipleSignals_CombineIntoHigherScore()
    {
        var lowRisk = _analyzer.Analyze("https://www.example.com/about");
        var highRisk = _analyzer.Analyze("http://192.168.1.5/account/verify.exe");

        Assert.True(highRisk.UrlAnalysisScore > lowRisk.UrlAnalysisScore);
    }
}
