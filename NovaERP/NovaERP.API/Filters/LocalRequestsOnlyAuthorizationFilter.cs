using System.Net;
using Hangfire.Dashboard;

namespace NovaERP.API.Filters;

/// <summary>Deliberate scope boundary: the Hangfire dashboard (mapped only in Development, see
/// Program.cs) is protected by the well-known Hangfire "local requests only" recipe rather than
/// real multi-user auth. A production deployment of this dashboard would need to be wired to the
/// existing JWT/permission system (e.g. require Authorize + a "scheduler.view" policy check
/// against the current ClaimsPrincipal) — that work is intentionally out of scope for now since
/// the dashboard isn't exposed outside Development.</summary>
public class LocalRequestsOnlyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress;

        if (remoteIp == null) return false;
        if (IPAddress.IsLoopback(remoteIp)) return true;

        // Covers the IPv4-mapped-to-IPv6 loopback form some hosts report (::ffff:127.0.0.1).
        var mapped = remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4() : remoteIp;
        return IPAddress.IsLoopback(mapped);
    }
}
