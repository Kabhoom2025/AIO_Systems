using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ILanguageService
{
    Task<List<LanguageDto>> GetAllAsync();
}
