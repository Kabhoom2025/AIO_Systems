namespace FoodOrder.Application.Interfaces;

/// <summary>
/// Reads the caller's tenant claims for the current request. Consumed by AppDbContext's
/// global query filters (Infrastructure) and by services that stamp a new row's
/// BranchId/OrganizationId on create. Implemented in the API layer (reads HttpContext);
/// Infrastructure/Application only ever see this abstraction.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>Null means SuperAdmin — sees/operates across every organization.</summary>
    int? OrganizationId { get; }

    /// <summary>Null means org-wide — e.g. an Admin who oversees every branch of their organization.</summary>
    int? BranchId { get; }
}
