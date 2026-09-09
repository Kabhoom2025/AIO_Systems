using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class RfqRequestService : IRfqRequestService
{
    private readonly IRfqRequestRepository _repo;
    private readonly IVendorRepository _vendorRepo;
    private readonly IAutomationEngine _automationEngine;
    private readonly IValidator<CreateRfqRequestDto> _createValidator;
    private readonly IValidator<UpdateRfqRequestDto> _updateValidator;
    private readonly IValidator<RecordRfqQuoteDto> _recordQuoteValidator;

    public RfqRequestService(IRfqRequestRepository repo, IVendorRepository vendorRepo, IAutomationEngine automationEngine,
        IValidator<CreateRfqRequestDto> createValidator, IValidator<UpdateRfqRequestDto> updateValidator,
        IValidator<RecordRfqQuoteDto> recordQuoteValidator)
    {
        _repo = repo;
        _vendorRepo = vendorRepo;
        _automationEngine = automationEngine;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _recordQuoteValidator = recordQuoteValidator;
    }

    public async Task<List<RfqRequestDto>> GetAllAsync(int orgId)
    {
        var rfqs = await _repo.GetAllByOrgAsync(orgId);
        return rfqs.Select(ToDto).ToList();
    }

    public async Task<RfqRequestDto> GetByIdAsync(int orgId, int id)
    {
        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");
        return ToDto(rfq);
    }

    public async Task<RfqRequestDto> CreateAsync(int orgId, CreateRfqRequestDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        await EnsureVendorsExistAsync(orgId, dto.InvitedVendorIds);

        var rfq = new RfqRequest
        {
            OrganizationId = orgId,
            Title = dto.Title,
            Description = dto.Description,
            Status = "Draft",
            IssueDate = dto.IssueDate,
            ResponseDeadline = dto.ResponseDeadline,
            OwnerId = dto.OwnerId,
            Items = dto.Items.Select(i => new RfqItem { ItemName = i.ItemName, Quantity = i.Quantity, DisplayOrder = i.DisplayOrder }).ToList(),
            Quotes = dto.InvitedVendorIds.Select(vendorId => new RfqVendorQuote { VendorId = vendorId }).ToList()
        };

        _repo.Add(rfq);
        await _repo.SaveChangesAsync();

        // RfqNumber depends on the generated Id, so it's set in a second save — same
        // two-phase scheme as SalesOrder.OrderNumber.
        rfq.RfqNumber = $"RFQ-{rfq.Id:D5}";
        _repo.Update(rfq);
        await _repo.SaveChangesAsync();

        return ToDto(rfq);
    }

    public async Task<RfqRequestDto> UpdateAsync(int orgId, int id, UpdateRfqRequestDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status != "Draft")
            throw new InvalidOperationException("Only draft RFQs can be edited.");

        await EnsureVendorsExistAsync(orgId, dto.InvitedVendorIds);

        rfq.Title = dto.Title;
        rfq.Description = dto.Description;
        rfq.IssueDate = dto.IssueDate;
        rfq.ResponseDeadline = dto.ResponseDeadline;
        rfq.OwnerId = dto.OwnerId;
        rfq.UpdatedDate = DateTime.UtcNow;

        rfq.Items.Clear();
        foreach (var i in dto.Items)
            rfq.Items.Add(new RfqItem { ItemName = i.ItemName, Quantity = i.Quantity, DisplayOrder = i.DisplayOrder });

        rfq.Quotes.Clear();
        foreach (var vendorId in dto.InvitedVendorIds)
            rfq.Quotes.Add(new RfqVendorQuote { VendorId = vendorId });

        _repo.Update(rfq);
        await _repo.SaveChangesAsync();
        return ToDto(rfq);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status != "Draft")
            throw new InvalidOperationException("Only draft RFQs can be deleted.");

        _repo.Remove(rfq);
        await _repo.SaveChangesAsync();
    }

    public async Task<RfqRequestDto> SendAsync(int orgId, int id)
    {
        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status != "Draft")
            throw new InvalidOperationException("Only draft RFQs can be sent.");

        rfq.Status = "Sent";
        rfq.UpdatedDate = DateTime.UtcNow;
        _repo.Update(rfq);
        await _repo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.RfqSent, new Dictionary<string, string>
        {
            ["EntityType"] = "RfqRequest",
            ["EntityId"] = rfq.Id.ToString(),
            ["RfqNumber"] = rfq.RfqNumber
        });

        return ToDto(rfq);
    }

    public async Task<RfqRequestDto> RecordQuoteAsync(int orgId, int id, RecordRfqQuoteDto dto)
    {
        await _recordQuoteValidator.ValidateAndThrowAsync(dto);

        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status != "Sent")
            throw new InvalidOperationException("Quotes can only be recorded while the RFQ is Sent.");

        var quote = rfq.Quotes.FirstOrDefault(q => q.VendorId == dto.VendorId)
            ?? throw new InvalidOperationException("This vendor was not invited to quote on this RFQ.");

        quote.QuotedAmount = dto.QuotedAmount;
        quote.Notes = dto.Notes;
        quote.RespondedDate = DateTime.UtcNow;

        _repo.Update(rfq);
        await _repo.SaveChangesAsync();
        return ToDto(rfq);
    }

    public async Task<RfqRequestDto> CloseAsync(int orgId, int id, CloseRfqRequestDto dto)
    {
        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status != "Sent")
            throw new InvalidOperationException("Only a sent RFQ can be closed.");

        if (dto.WinningVendorId.HasValue && rfq.Quotes.All(q => q.VendorId != dto.WinningVendorId.Value))
            throw new InvalidOperationException("The winning vendor must be one of the invited vendors.");

        rfq.Status = "Closed";
        rfq.WinningVendorId = dto.WinningVendorId;
        rfq.UpdatedDate = DateTime.UtcNow;
        _repo.Update(rfq);
        await _repo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.RfqClosed, new Dictionary<string, string>
        {
            ["EntityType"] = "RfqRequest",
            ["EntityId"] = rfq.Id.ToString(),
            ["RfqNumber"] = rfq.RfqNumber
        });

        return ToDto(rfq);
    }

    public async Task<RfqRequestDto> CancelAsync(int orgId, int id)
    {
        var rfq = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"RfqRequest {id} not found");

        if (rfq.Status is "Closed" or "Cancelled")
            throw new InvalidOperationException($"This RFQ is already {rfq.Status.ToLowerInvariant()}.");

        rfq.Status = "Cancelled";
        rfq.UpdatedDate = DateTime.UtcNow;
        _repo.Update(rfq);
        await _repo.SaveChangesAsync();
        return ToDto(rfq);
    }

    private async Task EnsureVendorsExistAsync(int orgId, List<int> vendorIds)
    {
        foreach (var vendorId in vendorIds)
        {
            if (await _vendorRepo.GetByIdAsync(orgId, vendorId) == null)
                throw new KeyNotFoundException($"Vendor {vendorId} not found");
        }
    }

    private static RfqRequestDto ToDto(RfqRequest r) => new()
    {
        Id = r.Id,
        RfqNumber = r.RfqNumber,
        Title = r.Title,
        Description = r.Description,
        Status = r.Status,
        IssueDate = r.IssueDate,
        ResponseDeadline = r.ResponseDeadline,
        OwnerId = r.OwnerId,
        OwnerName = r.Owner?.Name ?? string.Empty,
        WinningVendorId = r.WinningVendorId,
        WinningVendorName = r.WinningVendor?.Name,
        Items = r.Items.OrderBy(i => i.DisplayOrder).Select(i => new RfqItemDto
        {
            Id = i.Id,
            ItemName = i.ItemName,
            Quantity = i.Quantity,
            DisplayOrder = i.DisplayOrder
        }).ToList(),
        Quotes = r.Quotes.Select(q => new RfqVendorQuoteDto
        {
            Id = q.Id,
            VendorId = q.VendorId,
            VendorName = q.Vendor?.Name ?? string.Empty,
            QuotedAmount = q.QuotedAmount,
            Notes = q.Notes,
            RespondedDate = q.RespondedDate
        }).ToList()
    };
}
