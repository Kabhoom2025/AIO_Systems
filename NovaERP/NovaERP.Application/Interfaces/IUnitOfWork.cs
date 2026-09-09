namespace NovaERP.Application.Interfaces;

/// <summary>Thin abstraction over DbContext.SaveChangesAsync so services do not need to
/// depend on a specific repository's SaveChangesAsync to commit a unit of work spanning
/// multiple repositories.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
