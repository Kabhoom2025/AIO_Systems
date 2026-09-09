using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IDeliveryService
{
    Task<List<DeliveryDto>> GetAllAsync(int orgId);
    Task<DeliveryDto?> GetByIdAsync(int id);
    Task<DeliveryDto> CreateAsync(int orgId, CreateDeliveryDto dto);
    Task<DeliveryDto> AssignStaffAsync(int id, AssignDeliveryStaffDto dto);
    Task<DeliveryDto> UpdateStatusAsync(int id, UpdateDeliveryStatusDto dto);
    Task<DeliveryDto> VerifyOtpAsync(int id, VerifyDeliveryOtpDto dto);
}
