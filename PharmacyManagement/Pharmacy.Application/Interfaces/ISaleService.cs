using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface ISaleService
{
    Task<List<SaleDto>> GetAllAsync(int orgId);
    Task<SaleDto?> GetByIdAsync(int id);
    Task<SaleDto> CreateAsync(int orgId, CreateSaleDto dto);
    Task<SaleDto> CreateOnlineOrderAsync(int patientId, int orgId, CreateSaleDto dto);
    Task<List<SaleDto>> GetByPatientAsync(int patientId);
    Task<List<SaleDto>> GetOnlineOrdersAsync(int orgId);
    Task<SaleDto> FulfillOrderAsync(int saleId);
}
