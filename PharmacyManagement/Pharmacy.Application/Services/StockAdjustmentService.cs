using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IStockAdjustmentRepository _repo;

    public StockAdjustmentService(IStockAdjustmentRepository repo) => _repo = repo;

    public async Task<List<StockAdjustmentDto>> GetAllAsync(int orgId)
    {
        var adjustments = await _repo.GetAllByOrgAsync(orgId);
        return adjustments.Select(MapToDto).ToList();
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
    {
        var adjustment = await _repo.GetByIdAsync(id);
        return adjustment == null ? null : MapToDto(adjustment);
    }

    public async Task<StockAdjustmentDto> CreateAsync(int orgId, CreateStockAdjustmentDto dto)
    {
        if (dto.AdjustmentType is not ("Increase" or "Decrease"))
            throw new InvalidOperationException("AdjustmentType must be 'Increase' or 'Decrease'.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var batch = await _repo.GetBatchAsync(dto.MedicineBatchId)
            ?? throw new KeyNotFoundException($"Medicine batch {dto.MedicineBatchId} not found");

        var newQuantity = dto.AdjustmentType == "Increase"
            ? batch.CurrentQuantity + dto.Quantity
            : batch.CurrentQuantity - dto.Quantity;

        if (newQuantity < 0)
            throw new InvalidOperationException("Adjustment would result in negative stock.");

        batch.CurrentQuantity = newQuantity;
        batch.UpdatedDate     = DateTime.UtcNow;

        var adjustment = new StockAdjustment
        {
            OrganizationId  = orgId,
            MedicineId      = dto.MedicineId,
            MedicineBatchId = dto.MedicineBatchId,
            MedicineBatch   = batch,
            AdjustmentType  = dto.AdjustmentType,
            Quantity        = dto.Quantity,
            Reason          = dto.Reason,
            Notes           = dto.Notes,
            AdjustedDate    = DateTime.UtcNow
        };

        _repo.Add(adjustment);
        await _repo.SaveChangesAsync();
        return MapToDto(adjustment);
    }

    private static StockAdjustmentDto MapToDto(StockAdjustment a) => new()
    {
        Id              = a.Id,
        MedicineId      = a.MedicineId,
        MedicineName    = a.Medicine?.Name ?? string.Empty,
        MedicineBatchId = a.MedicineBatchId,
        BatchNumber     = a.MedicineBatch?.BatchNumber ?? string.Empty,
        AdjustmentType  = a.AdjustmentType,
        Quantity        = a.Quantity,
        Reason          = a.Reason,
        Notes           = a.Notes,
        AdjustedDate    = a.AdjustedDate
    };
}
