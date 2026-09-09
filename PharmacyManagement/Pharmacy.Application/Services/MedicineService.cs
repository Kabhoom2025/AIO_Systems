using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class MedicineService : IMedicineService
{
    private readonly IMedicineRepository _repo;

    public MedicineService(IMedicineRepository repo) => _repo = repo;

    public async Task<List<MedicineDto>> GetAllAsync(int orgId)
    {
        var medicines = await _repo.GetAllByOrgAsync(orgId);
        return medicines.Select(MapToDto).ToList();
    }

    public async Task<List<PublicMedicineDto>> GetPublicListAsync(int orgId)
    {
        var medicines = await _repo.GetAllByOrgAsync(orgId);
        return medicines
            .Where(m => m.IsActive)
            .Select(m => new { m, stock = m.Batches.Sum(b => b.CurrentQuantity) })
            .Where(x => x.stock > 0)
            .Select(x => new PublicMedicineDto
            {
                Id          = x.m.Id,
                Name        = x.m.Name,
                GenericName = x.m.GenericName,
                Category    = x.m.Category,
                MRP         = x.m.MRP,
                Unit        = x.m.Unit,
                PackSize    = x.m.PackSize,
                TotalStock  = x.stock
            })
            .ToList();
    }

    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var m = await _repo.GetByIdAsync(id);
        return m == null ? null : MapToDto(m);
    }

    public async Task<MedicineDto?> GetByCodeAsync(int orgId, string code)
    {
        var m = await _repo.GetByCodeAsync(orgId, code);
        return m == null ? null : MapToDto(m);
    }

    public async Task<MedicineDto> CreateAsync(int orgId, CreateMedicineDto dto)
    {
        var medicine = new Medicine
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            GenericName    = dto.GenericName,
            Category       = dto.Category,
            DrugSchedule   = dto.DrugSchedule,
            PackType       = dto.PackType,
            PackSize       = dto.PackSize,
            Unit           = dto.Unit,
            Manufacturer   = dto.Manufacturer,
            HsnCode        = dto.HsnCode,
            MRP            = dto.MRP,
            PurchasePrice  = dto.PurchasePrice,
            GstPercent     = dto.GstPercent,
            RackLocation   = dto.RackLocation,
            ReorderLevel   = dto.ReorderLevel,
            IsActive       = true
        };
        _repo.Add(medicine);
        await _repo.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(dto.Sku) || string.IsNullOrWhiteSpace(dto.Barcode))
        {
            medicine.Sku     = string.IsNullOrWhiteSpace(dto.Sku) ? $"SKU-{medicine.Id:D6}" : dto.Sku;
            medicine.Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? $"{medicine.Id:D12}" : dto.Barcode;
            await _repo.SaveChangesAsync();
        }

        return MapToDto(medicine);
    }

    public async Task<MedicineDto> UpdateAsync(int id, UpdateMedicineDto dto)
    {
        var medicine = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medicine {id} not found");

        medicine.Name          = dto.Name;
        medicine.GenericName   = dto.GenericName;
        medicine.Category      = dto.Category;
        medicine.DrugSchedule  = dto.DrugSchedule;
        medicine.PackType      = dto.PackType;
        medicine.PackSize      = dto.PackSize;
        medicine.Unit          = dto.Unit;
        medicine.Manufacturer  = dto.Manufacturer;
        medicine.HsnCode       = dto.HsnCode;
        if (!string.IsNullOrWhiteSpace(dto.Sku)) medicine.Sku = dto.Sku;
        if (!string.IsNullOrWhiteSpace(dto.Barcode)) medicine.Barcode = dto.Barcode;
        medicine.MRP           = dto.MRP;
        medicine.PurchasePrice = dto.PurchasePrice;
        medicine.GstPercent    = dto.GstPercent;
        medicine.RackLocation  = dto.RackLocation;
        medicine.ReorderLevel  = dto.ReorderLevel;
        medicine.IsActive      = dto.IsActive;
        medicine.UpdatedDate   = DateTime.UtcNow;

        _repo.Update(medicine);
        await _repo.SaveChangesAsync();
        return MapToDto(medicine);
    }

    public async Task DeleteAsync(int id)
    {
        var medicine = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medicine {id} not found");
        _repo.Remove(medicine);
        await _repo.SaveChangesAsync();
    }

    public async Task<MedicineBatchDto> AddBatchAsync(int medicineId, AddBatchDto dto)
    {
        var medicine = await _repo.GetByIdAsync(medicineId)
            ?? throw new KeyNotFoundException($"Medicine {medicineId} not found");

        var batch = new MedicineBatch
        {
            MedicineId         = medicineId,
            BatchNumber        = dto.BatchNumber,
            ExpiryDate         = DateTime.SpecifyKind(dto.ExpiryDate, DateTimeKind.Utc),
            ManufacturingDate  = dto.ManufacturingDate.HasValue
                ? DateTime.SpecifyKind(dto.ManufacturingDate.Value, DateTimeKind.Utc)
                : null,
            QuantityReceived   = dto.Quantity,
            CurrentQuantity    = dto.Quantity,
            PurchasePrice      = dto.PurchasePrice
        };
        _repo.AddBatch(batch);
        await _repo.SaveChangesAsync();
        return MapBatchToDto(batch);
    }

    public async Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync(int orgId, int daysThreshold = 90)
    {
        var batches = await _repo.GetExpiryAlertsAsync(orgId, daysThreshold);
        return batches.Select(b =>
        {
            var days = (b.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
            return new ExpiryAlertDto
            {
                MedicineId   = b.MedicineId,
                MedicineName = b.Medicine.Name,
                GenericName  = b.Medicine.GenericName,
                BatchId      = b.Id,
                BatchNumber  = b.BatchNumber,
                ExpiryDate   = b.ExpiryDate,
                DaysToExpiry = days,
                Quantity     = b.CurrentQuantity,
                ExpiryStatus = GetExpiryStatus(days),
                RackLocation = b.Medicine.RackLocation
            };
        }).OrderBy(a => a.DaysToExpiry).ToList();
    }

    private static string GetExpiryStatus(int days) => days switch
    {
        < 0  => "expired",
        < 30 => "critical",
        < 90 => "warning",
        _    => "ok"
    };

    private static MedicineDto MapToDto(Medicine m) => new()
    {
        Id            = m.Id,
        Name          = m.Name,
        GenericName   = m.GenericName,
        Category      = m.Category,
        DrugSchedule  = m.DrugSchedule,
        PackType      = m.PackType,
        PackSize      = m.PackSize,
        Unit          = m.Unit,
        Manufacturer  = m.Manufacturer,
        HsnCode       = m.HsnCode,
        Sku           = m.Sku,
        Barcode       = m.Barcode,
        MRP           = m.MRP,
        PurchasePrice = m.PurchasePrice,
        GstPercent    = m.GstPercent,
        RackLocation  = m.RackLocation,
        ReorderLevel  = m.ReorderLevel,
        IsActive      = m.IsActive,
        TotalStock    = m.Batches.Sum(b => b.CurrentQuantity),
        Batches       = m.Batches.Select(MapBatchToDto).ToList()
    };

    private static MedicineBatchDto MapBatchToDto(MedicineBatch b)
    {
        var days = (b.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        return new MedicineBatchDto
        {
            Id                = b.Id,
            MedicineId        = b.MedicineId,
            BatchNumber       = b.BatchNumber,
            ExpiryDate        = b.ExpiryDate,
            ManufacturingDate = b.ManufacturingDate,
            QuantityReceived  = b.QuantityReceived,
            CurrentQuantity   = b.CurrentQuantity,
            PurchasePrice     = b.PurchasePrice,
            DaysToExpiry      = days,
            ExpiryStatus      = days switch { < 0 => "expired", < 30 => "critical", < 90 => "warning", _ => "ok" }
        };
    }
}
