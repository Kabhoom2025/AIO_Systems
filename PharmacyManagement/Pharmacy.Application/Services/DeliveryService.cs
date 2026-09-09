using System.Security.Cryptography;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class DeliveryService : IDeliveryService
{
    private static readonly string[] AllowedStatuses = { "Pending", "Assigned", "OutForDelivery", "Delivered", "Cancelled" };

    private readonly IDeliveryRepository _repo;

    public DeliveryService(IDeliveryRepository repo) => _repo = repo;

    public async Task<List<DeliveryDto>> GetAllAsync(int orgId)
    {
        var deliveries = await _repo.GetAllByOrgAsync(orgId);
        return deliveries.Select(MapToDto).ToList();
    }

    public async Task<DeliveryDto?> GetByIdAsync(int id)
    {
        var delivery = await _repo.GetByIdAsync(id);
        return delivery == null ? null : MapToDto(delivery);
    }

    public async Task<DeliveryDto> CreateAsync(int orgId, CreateDeliveryDto dto)
    {
        var sale = await _repo.GetSaleAsync(dto.SaleId)
            ?? throw new KeyNotFoundException($"Sale {dto.SaleId} not found");

        var delivery = new Delivery
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            SaleId         = dto.SaleId,
            Sale           = sale,
            CustomerId     = dto.CustomerId,
            Address        = dto.Address,
            ScheduledDate  = DateTime.SpecifyKind(dto.ScheduledDate, DateTimeKind.Utc),
            DeliveryCharge = dto.DeliveryCharge,
            Status         = "Pending",
            OtpCode        = GenerateOtp(),
            Notes          = dto.Notes
        };

        _repo.Add(delivery);
        await _repo.SaveChangesAsync();
        return MapToDto(delivery);
    }

    public async Task<DeliveryDto> AssignStaffAsync(int id, AssignDeliveryStaffDto dto)
    {
        var delivery = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Delivery {id} not found");

        if (delivery.Status is not ("Pending" or "Assigned"))
            throw new InvalidOperationException($"Cannot assign staff to a delivery with status '{delivery.Status}'.");

        delivery.DeliveryStaffId = dto.DeliveryStaffId;
        delivery.Status = "Assigned";
        delivery.UpdatedDate = DateTime.UtcNow;

        await _repo.SaveChangesAsync();
        return MapToDto(delivery);
    }

    public async Task<DeliveryDto> UpdateStatusAsync(int id, UpdateDeliveryStatusDto dto)
    {
        var delivery = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Delivery {id} not found");

        if (!AllowedStatuses.Contains(dto.Status))
            throw new InvalidOperationException($"Unknown status '{dto.Status}'.");

        var validTransitions = new Dictionary<string, string[]>
        {
            ["Pending"]        = new[] { "Cancelled" },
            ["Assigned"]       = new[] { "OutForDelivery", "Cancelled" },
            ["OutForDelivery"] = new[] { "Cancelled" }
        };

        if (!validTransitions.TryGetValue(delivery.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException($"Cannot transition delivery from '{delivery.Status}' to '{dto.Status}'.");

        delivery.Status = dto.Status;
        delivery.UpdatedDate = DateTime.UtcNow;

        await _repo.SaveChangesAsync();
        return MapToDto(delivery);
    }

    public async Task<DeliveryDto> VerifyOtpAsync(int id, VerifyDeliveryOtpDto dto)
    {
        var delivery = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Delivery {id} not found");

        if (delivery.Status != "OutForDelivery")
            throw new InvalidOperationException("OTP can only be verified while the delivery is out for delivery.");

        if (delivery.OtpCode != dto.OtpCode)
            throw new InvalidOperationException("Incorrect OTP.");

        delivery.OtpVerifiedDate = DateTime.UtcNow;
        delivery.Status = "Delivered";
        delivery.UpdatedDate = DateTime.UtcNow;

        await _repo.SaveChangesAsync();
        return MapToDto(delivery);
    }

    private static string GenerateOtp() => RandomNumberGenerator.GetInt32(100000, 999999).ToString();

    private static DeliveryDto MapToDto(Delivery d) => new()
    {
        Id                = d.Id,
        SaleId            = d.SaleId,
        SaleInvoiceNumber = d.Sale?.InvoiceNumber ?? string.Empty,
        BranchName        = d.Branch?.Name ?? string.Empty,
        CustomerId        = d.CustomerId,
        CustomerName      = d.Customer?.Name,
        DeliveryStaffId   = d.DeliveryStaffId,
        DeliveryStaffName = d.DeliveryStaff?.Name,
        Address           = d.Address,
        ScheduledDate     = d.ScheduledDate,
        DeliveryCharge    = d.DeliveryCharge,
        Status            = d.Status,
        OtpCode           = d.OtpCode,
        OtpVerifiedDate   = d.OtpVerifiedDate,
        Notes             = d.Notes
    };
}
