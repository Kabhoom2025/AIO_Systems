using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly NovaErpDbContext _ctx;

    public LanguageRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Language>> GetAllActiveAsync() =>
        _ctx.Languages.Where(l => l.IsActive).OrderBy(l => l.Name).ToListAsync();
}
