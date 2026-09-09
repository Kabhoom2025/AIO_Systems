using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

public class ScheduledJobService : IScheduledJobService
{
    private readonly IScheduledJobRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateScheduledJobDto> _updateValidator;

    public ScheduledJobService(IScheduledJobRepository repo, IMapper mapper, IValidator<UpdateScheduledJobDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<List<ScheduledJobDefinitionDto>> GetAllAsync(int orgId)
    {
        var jobs = await _repo.GetVisibleToOrgAsync(orgId);
        return jobs.Select(_mapper.Map<ScheduledJobDefinitionDto>).ToList();
    }

    public async Task<ScheduledJobDefinitionDto> GetByIdAsync(int orgId, int id)
    {
        var job = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Scheduled job {id} not found");
        return _mapper.Map<ScheduledJobDefinitionDto>(job);
    }

    public async Task<ScheduledJobDefinitionDto> UpdateAsync(int orgId, int id, UpdateScheduledJobDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var job = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Scheduled job {id} not found");

        job.CronExpression = dto.CronExpression;
        job.IsEnabled = dto.IsEnabled;
        job.UpdatedDate = DateTime.UtcNow;

        _repo.Update(job);
        await _repo.SaveChangesAsync();
        return _mapper.Map<ScheduledJobDefinitionDto>(job);
    }
}
