using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class SaleService : ISaleService
{
    private static readonly string[] AllowedPaymentMethods = { "Cash", "Card", "UPI", "Wallet", "Credit" };

    private readonly ISaleRepository _repo;

    public SaleService(ISaleRepository repo) => _repo = repo;

    public async Task<List<SaleDto>> GetAllAsync(int orgId)
    {
        var sales = await _repo.GetAllByOrgAsync(orgId);
        return sales.Select(MapToDto).ToList();
    }

    public async Task<SaleDto?> GetByIdAsync(int id)
    {
        var sale = await _repo.GetByIdAsync(id);
        return sale == null ? null : MapToDto(sale);
    }

    public Task<SaleDto> CreateAsync(int orgId, CreateSaleDto dto) =>
        BuildAndSaveSaleAsync(orgId, dto, patientId: null, channel: "POS");

    public Task<SaleDto> CreateOnlineOrderAsync(int patientId, int orgId, CreateSaleDto dto) =>
        BuildAndSaveSaleAsync(orgId, dto, patientId, channel: "Online");

    public async Task<List<SaleDto>> GetByPatientAsync(int patientId)
    {
        var sales = await _repo.GetByPatientAsync(patientId);
        return sales.Select(MapToDto).ToList();
    }

    public async Task<List<SaleDto>> GetOnlineOrdersAsync(int orgId)
    {
        var sales = await _repo.GetAllByOrgAsync(orgId);
        return sales.Where(s => s.Channel == "Online").Select(MapToDto).ToList();
    }

    public async Task<SaleDto> FulfillOrderAsync(int saleId)
    {
        var sale = await _repo.GetByIdAsync(saleId)
            ?? throw new KeyNotFoundException($"Sale {saleId} not found");

        if (sale.Channel != "Online")
            throw new InvalidOperationException("Only online orders can be fulfilled.");

        if (sale.Status != "Pending")
            throw new InvalidOperationException($"Order is already {sale.Status}.");

        sale.Status = "Completed";
        sale.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapToDto(sale);
    }

    private async Task<SaleDto> BuildAndSaveSaleAsync(int orgId, CreateSaleDto dto, int? patientId, string channel)
    {
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("A sale must have at least one item.");

        if (!AllowedPaymentMethods.Contains(dto.PaymentMethod))
            throw new InvalidOperationException($"Unknown payment method '{dto.PaymentMethod}'.");

        var sale = new Sale
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            CustomerId     = dto.CustomerId,
            PatientId      = patientId,
            Channel        = channel,
            InvoiceNumber  = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            SaleDate       = DateTime.UtcNow,
            PaymentMethod  = dto.PaymentMethod,
            Status         = channel == "Online" ? "Pending" : "Completed"
        };

        foreach (var line in dto.Items)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException("Item quantity must be greater than zero.");

            var medicine = await _repo.GetMedicineWithBatchesAsync(line.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {line.MedicineId} not found");

            var remaining = line.Quantity;
            var batches = medicine.Batches.OrderBy(b => b.ExpiryDate).ToList();
            var consumedAny = false;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                var take = Math.Min(remaining, batch.CurrentQuantity);
                if (take <= 0) continue;

                batch.CurrentQuantity -= take;
                batch.UpdatedDate = DateTime.UtcNow;
                remaining -= take;
                consumedAny = true;

                var lineSubtotal = take * medicine.MRP;
                var lineDiscount = line.DiscountAmount * take / line.Quantity;
                var taxable = lineSubtotal - lineDiscount;
                var lineTax = taxable * medicine.GstPercent / 100;

                sale.Items.Add(new SaleItem
                {
                    MedicineId      = medicine.Id,
                    MedicineBatchId = batch.Id,
                    Quantity        = take,
                    UnitPrice       = medicine.MRP,
                    DiscountAmount  = lineDiscount,
                    GstPercent      = medicine.GstPercent,
                    LineTotal       = taxable + lineTax
                });

                sale.Subtotal       += lineSubtotal;
                sale.DiscountAmount += lineDiscount;
                sale.TaxAmount      += lineTax;
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException(consumedAny
                    ? $"Insufficient stock for '{medicine.Name}' — only {line.Quantity - remaining} of {line.Quantity} available."
                    : $"'{medicine.Name}' is out of stock.");
            }
        }

        sale.TotalAmount = sale.Subtotal - sale.DiscountAmount + sale.TaxAmount;

        if (dto.CustomerId.HasValue)
        {
            var customer = await _repo.GetCustomerAsync(dto.CustomerId.Value);
            if (customer != null)
            {
                customer.LoyaltyPoints += (int)(sale.TotalAmount / 100);
                customer.UpdatedDate = DateTime.UtcNow;
            }
        }

        _repo.Add(sale);
        await _repo.SaveChangesAsync();
        return MapToDto(sale);
    }

    private static SaleDto MapToDto(Sale s) => new()
    {
        Id             = s.Id,
        InvoiceNumber  = s.InvoiceNumber,
        SaleDate       = s.SaleDate,
        BranchId       = s.BranchId,
        BranchName     = s.Branch?.Name ?? string.Empty,
        CustomerId     = s.CustomerId,
        CustomerName   = s.Customer?.Name,
        PatientId      = s.PatientId,
        PatientName    = s.Patient?.Name,
        PatientPhone   = s.Patient?.Phone,
        Channel        = s.Channel,
        Subtotal       = s.Subtotal,
        DiscountAmount = s.DiscountAmount,
        TaxAmount      = s.TaxAmount,
        TotalAmount    = s.TotalAmount,
        PaymentMethod  = s.PaymentMethod,
        Status         = s.Status,
        Items = s.Items.Select(i => new SaleItemDto
        {
            Id             = i.Id,
            MedicineId     = i.MedicineId,
            MedicineName   = i.Medicine?.Name ?? string.Empty,
            BatchNumber    = i.MedicineBatch?.BatchNumber ?? string.Empty,
            Quantity       = i.Quantity,
            UnitPrice      = i.UnitPrice,
            DiscountAmount = i.DiscountAmount,
            GstPercent     = i.GstPercent,
            LineTotal      = i.LineTotal
        }).ToList()
    };
}
