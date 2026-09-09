using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FlowSphere.Infrastructure.Authentication;

/// <summary>Reads the sandbox stage the caller is working in from the X-Environment-Stage
/// request header. Defaults to Live when the header is absent or unrecognized - a client that
/// forgets to send it sees production data, not accidentally-exposed Dev/QA test data.</summary>
public class CurrentEnvironmentContext : ICurrentEnvironmentContext
{
    private const string HeaderName = "X-Environment-Stage";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentEnvironmentContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public EnvironmentStage Stage
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();
            return Enum.TryParse<EnvironmentStage>(header, ignoreCase: true, out var stage) ? stage : EnvironmentStage.Live;
        }
    }
}
