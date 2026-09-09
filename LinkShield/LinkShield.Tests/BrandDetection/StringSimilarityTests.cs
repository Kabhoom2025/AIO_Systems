using LinkShield.Application.Common;
using Xunit;

namespace LinkShield.Tests.BrandDetection;

public class StringSimilarityTests
{
    [Theory]
    [InlineData("paypal.com", "paypal.com", 0)]
    [InlineData("paypal.com", "paypal1.com", 1)]
    [InlineData("paypal.com", "paypa1.com", 1)]
    [InlineData("kitten", "sitting", 3)]
    public void LevenshteinDistance_MatchesKnownValues(string a, string b, int expected)
    {
        Assert.Equal(expected, StringSimilarity.LevenshteinDistance(a, b));
    }

    [Fact]
    public void JaroWinklerSimilarity_IdenticalStrings_ReturnsOne()
    {
        Assert.Equal(1.0, StringSimilarity.JaroWinklerSimilarity("microsoft.com", "microsoft.com"), precision: 5);
    }

    [Fact]
    public void JaroWinklerSimilarity_TotallyDifferentStrings_IsLow()
    {
        var similarity = StringSimilarity.JaroWinklerSimilarity("microsoft.com", "zzzzzzzzzzzz");
        Assert.True(similarity < 0.5);
    }

    [Fact]
    public void JaroWinklerSimilarity_TyposquatIsHighlySimilar()
    {
        var similarity = StringSimilarity.JaroWinklerSimilarity("paypa1.com", "paypal.com");
        Assert.True(similarity > 0.9);
    }
}
