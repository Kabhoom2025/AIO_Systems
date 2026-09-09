using System.Net;
using LinkShield.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LinkShield.Tests.Security;

public class SsrfGuardTests
{
    private static SsrfGuard CreateGuard() => new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["SsrfProtection:RequestTimeoutMs"] = "2000" })
        .Build());

    [Theory]
    [InlineData("127.0.0.1")]      // loopback
    [InlineData("10.0.0.5")]       // RFC1918 private
    [InlineData("172.16.0.1")]     // RFC1918 private
    [InlineData("192.168.1.1")]    // RFC1918 private
    [InlineData("169.254.169.254")] // cloud metadata / link-local
    [InlineData("0.0.0.0")]
    [InlineData("224.0.0.1")]      // multicast
    public void IsDisallowedAddress_BlocksPrivateAndReservedIPv4(string ip)
    {
        var guard = CreateGuard();
        Assert.True(guard.IsDisallowedAddress(IPAddress.Parse(ip)));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("93.184.216.34")]
    public void IsDisallowedAddress_AllowsPublicIPv4(string ip)
    {
        var guard = CreateGuard();
        Assert.False(guard.IsDisallowedAddress(IPAddress.Parse(ip)));
    }

    [Theory]
    [InlineData("::1")]        // loopback
    [InlineData("fe80::1")]    // link-local
    [InlineData("fc00::1")]    // unique local
    public void IsDisallowedAddress_BlocksReservedIPv6(string ip)
    {
        var guard = CreateGuard();
        Assert.True(guard.IsDisallowedAddress(IPAddress.Parse(ip)));
    }

    [Fact]
    public async Task ValidateHostAsync_DeniesLiteralPrivateIp()
    {
        var guard = CreateGuard();
        var result = await guard.ValidateHostAsync("192.168.1.10");
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_DeniesSingleLabelHostname()
    {
        var guard = CreateGuard();
        var result = await guard.ValidateHostAsync("localhost");
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_DeniesDotInternalSuffix()
    {
        var guard = CreateGuard();
        var result = await guard.ValidateHostAsync("service.internal");
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_DeniesMetadataHostname()
    {
        var guard = CreateGuard();
        var result = await guard.ValidateHostAsync("metadata.google.internal");
        Assert.False(result.IsAllowed);
    }
}
