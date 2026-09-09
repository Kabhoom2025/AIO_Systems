using NovaERP.Application.Interfaces;

namespace NovaERP.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly NovaErpDbContext _ctx;

    public UnitOfWork(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
