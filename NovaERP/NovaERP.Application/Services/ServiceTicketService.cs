using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ServiceTicketService : IServiceTicketService
{
    private readonly IServiceTicketRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IValidator<CreateServiceTicketDto> _createValidator;
    private readonly IValidator<UpdateServiceTicketDto> _updateValidator;
    private readonly IValidator<AssignTicketDto> _assignValidator;
    private readonly IValidator<ResolveTicketDto> _resolveValidator;

    public ServiceTicketService(IServiceTicketRepository repo, IEmployeeRepository employeeRepo,
        IValidator<CreateServiceTicketDto> createValidator, IValidator<UpdateServiceTicketDto> updateValidator,
        IValidator<AssignTicketDto> assignValidator, IValidator<ResolveTicketDto> resolveValidator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
        _resolveValidator = resolveValidator;
    }

    public async Task<List<ServiceTicketDto>> GetAllAsync(int orgId)
    {
        var tickets = await _repo.GetAllByOrgAsync(orgId);
        return tickets.Select(ToDto).ToList();
    }

    public async Task<ServiceTicketDto> GetByIdAsync(int orgId, int id)
    {
        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");
        return ToDto(ticket);
    }

    public async Task<ServiceTicketDto> CreateAsync(int orgId, CreateServiceTicketDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        _ = await _employeeRepo.GetByIdAsync(orgId, dto.RequesterId)
            ?? throw new KeyNotFoundException($"Employee {dto.RequesterId} not found");

        var ticket = new ServiceTicket
        {
            OrganizationId = orgId,
            Subject = dto.Subject,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            RequesterId = dto.RequesterId,
            Priority = dto.Priority,
            Status = "Open"
        };

        _repo.Add(ticket);
        await _repo.SaveChangesAsync();

        // TicketNumber depends on the generated Id, so it's set in a second save — same scheme
        // as Asset.AssetCode/Employee.EmployeeCode.
        ticket.TicketNumber = $"TCK-{ticket.Id:D5}";
        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, ticket.Id) ?? ticket;
        return ToDto(reloaded);
    }

    public async Task<ServiceTicketDto> UpdateAsync(int orgId, int id, UpdateServiceTicketDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status == "Closed")
            throw new InvalidOperationException("Closed tickets cannot be edited.");

        ticket.Subject = dto.Subject;
        ticket.Description = dto.Description;
        ticket.CategoryId = dto.CategoryId;
        ticket.Priority = dto.Priority;
        ticket.UpdatedDate = DateTime.UtcNow;

        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, ticket.Id) ?? ticket;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status == "Closed")
            throw new InvalidOperationException("Closed tickets cannot be deleted.");

        _repo.Remove(ticket);
        await _repo.SaveChangesAsync();
    }

    public async Task<ServiceTicketDto> AssignAsync(int orgId, int id, AssignTicketDto dto)
    {
        await _assignValidator.ValidateAndThrowAsync(dto);

        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status != "Open" && ticket.Status != "InProgress")
            throw new InvalidOperationException("Only open or in-progress tickets can be assigned.");

        _ = await _employeeRepo.GetByIdAsync(orgId, dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");

        ticket.AssignedToId = dto.EmployeeId;
        ticket.Status = "InProgress";
        ticket.UpdatedDate = DateTime.UtcNow;
        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, ticket.Id) ?? ticket;
        return ToDto(reloaded);
    }

    public async Task<ServiceTicketDto> ResolveAsync(int orgId, int id, ResolveTicketDto dto)
    {
        await _resolveValidator.ValidateAndThrowAsync(dto);

        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status != "Open" && ticket.Status != "InProgress")
            throw new InvalidOperationException("Only open or in-progress tickets can be resolved.");

        ticket.Status = "Resolved";
        ticket.ResolutionNotes = dto.ResolutionNotes;
        ticket.ResolvedDate = DateTime.UtcNow;
        ticket.UpdatedDate = DateTime.UtcNow;
        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, ticket.Id) ?? ticket;
        return ToDto(reloaded);
    }

    public async Task<ServiceTicketDto> CloseAsync(int orgId, int id)
    {
        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status != "Resolved")
            throw new InvalidOperationException("Only resolved tickets can be closed.");

        ticket.Status = "Closed";
        ticket.ClosedDate = DateTime.UtcNow;
        ticket.UpdatedDate = DateTime.UtcNow;
        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        return ToDto(ticket);
    }

    public async Task<ServiceTicketDto> ReopenAsync(int orgId, int id)
    {
        var ticket = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ServiceTicket {id} not found");

        if (ticket.Status != "Resolved")
            throw new InvalidOperationException("Only resolved tickets can be reopened.");

        ticket.Status = ticket.AssignedToId.HasValue ? "InProgress" : "Open";
        ticket.ResolvedDate = null;
        ticket.UpdatedDate = DateTime.UtcNow;
        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        return ToDto(ticket);
    }

    private static ServiceTicketDto ToDto(ServiceTicket t) => new()
    {
        Id = t.Id,
        TicketNumber = t.TicketNumber,
        Subject = t.Subject,
        Description = t.Description,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name ?? string.Empty,
        RequesterId = t.RequesterId,
        RequesterName = t.Requester != null ? $"{t.Requester.FirstName} {t.Requester.LastName}" : string.Empty,
        AssignedToId = t.AssignedToId,
        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
        Priority = t.Priority,
        Status = t.Status,
        ResolutionNotes = t.ResolutionNotes,
        ResolvedDate = t.ResolvedDate,
        ClosedDate = t.ClosedDate
    };
}
