using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ILanguageRepository
{
    Task<List<Language>> GetAllActiveAsync();
}
