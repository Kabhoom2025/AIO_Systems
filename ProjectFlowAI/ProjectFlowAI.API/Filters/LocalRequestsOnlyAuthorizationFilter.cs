using Hangfire.Dashboard;

namespace ProjectFlowAI.API.Filters;

/// <summary>Guards the Hangfire dashboard (Development only — see Program.cs) with a
/// local-requests-only check rather than real multi-user auth. This is a deliberate, documented
/// scope boundary mirroring NovaERP's identical filter: a real production deployment of this
/// dashboard would need real auth wired to the JWT/permission system.</summary>
public class LocalRequestsOnlyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.Connection.LocalIpAddress?.Equals(httpContext.Connection.RemoteIpAddress) == true
            || System.Net.IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress ?? System.Net.IPAddress.None);
    }
}
