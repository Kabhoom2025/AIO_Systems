using AutoMapper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _repo;
    private readonly IMapper _mapper;

    public LanguageService(ILanguageRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<LanguageDto>> GetAllAsync()
    {
        var languages = await _repo.GetAllActiveAsync();
        return languages.Select(_mapper.Map<LanguageDto>).ToList();
    }
}
