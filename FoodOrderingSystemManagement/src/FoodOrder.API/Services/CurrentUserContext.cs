using FoodOrder.Application.Interfaces;

namespace FoodOrder.API.Services;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public int? OrganizationId => ReadClaim("organizationId");
    public int? BranchId => ReadClaim("branchId");

    private int? ReadClaim(string claimType)
    {
        var value = httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        return value != null && int.TryParse(value, out var id) ? id : null;
    }
}
