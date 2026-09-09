using FlowSphere.Application.Apps.Common;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class AppSubmissionValidationTests
{
    private const string RestrictSettings = """
    {"attributes":{"uniqueRecord":{"enabled":true,"type":"Restrict","fieldKeys":["email"],"message":"The '{fields}' must be unique and cannot have duplicates."}}}
    """;
    private const string WarnSettings = """
    {"attributes":{"uniqueRecord":{"enabled":true,"type":"Warn","fieldKeys":["email"]}}}
    """;
    private const string DisabledSettings = "{}";

    [Fact]
    public void CheckUniqueRecord_RestrictTypeWithDuplicate_ReturnsRestricted()
    {
        var result = AppSubmissionValidation.CheckUniqueRecord(
            RestrictSettings, """{"email":"a@b.com"}""", new[] { """{"email":"a@b.com"}""" });

        Assert.True(result.Restricted);
        Assert.False(result.Warned);
        Assert.Contains("email", result.Message);
    }

    [Fact]
    public void CheckUniqueRecord_WarnTypeWithDuplicate_ReturnsWarnedNotRestricted()
    {
        var result = AppSubmissionValidation.CheckUniqueRecord(
            WarnSettings, """{"email":"a@b.com"}""", new[] { """{"email":"a@b.com"}""" });

        Assert.False(result.Restricted);
        Assert.True(result.Warned);
    }

    [Fact]
    public void CheckUniqueRecord_NoDuplicate_ReturnsNeitherRestrictedNorWarned()
    {
        var result = AppSubmissionValidation.CheckUniqueRecord(
            RestrictSettings, """{"email":"new@b.com"}""", new[] { """{"email":"a@b.com"}""" });

        Assert.False(result.Restricted);
        Assert.False(result.Warned);
    }

    [Fact]
    public void CheckUniqueRecord_Disabled_AlwaysPasses()
    {
        var result = AppSubmissionValidation.CheckUniqueRecord(
            DisabledSettings, """{"email":"a@b.com"}""", new[] { """{"email":"a@b.com"}""" });

        Assert.False(result.Restricted);
        Assert.False(result.Warned);
    }

    [Fact]
    public void IsCaptchaRequiredOnSubmit_WhenBoth_ReturnsTrue()
    {
        var settings = """{"accessibility":{"captcha":{"enabled":true,"when":"Both"}}}""";
        Assert.True(AppSubmissionValidation.IsCaptchaRequiredOnSubmit(settings));
    }

    [Fact]
    public void IsCaptchaRequiredOnSubmit_WhenBeforeLoadOnly_ReturnsFalse()
    {
        var settings = """{"accessibility":{"captcha":{"enabled":true,"when":"BeforeLoad"}}}""";
        Assert.False(AppSubmissionValidation.IsCaptchaRequiredOnSubmit(settings));
    }

    [Fact]
    public void IsOtpRequiredOnSubmit_Disabled_ReturnsFalse()
    {
        var settings = """{"accessibility":{"otp":{"enabled":false,"when":"OnSubmission"}}}""";
        Assert.False(AppSubmissionValidation.IsOtpRequiredOnSubmit(settings));
    }

    [Fact]
    public void ReadSharedAccess_GuestEnabled_ReturnsTokenAndEnabled()
    {
        var settings = """{"shared":{"guestLinkEnabled":true,"guestToken":"abc123"}}""";
        var (enabled, token) = AppSubmissionValidation.ReadSharedAccess(settings, "guest");

        Assert.True(enabled);
        Assert.Equal("abc123", token);
    }
}
