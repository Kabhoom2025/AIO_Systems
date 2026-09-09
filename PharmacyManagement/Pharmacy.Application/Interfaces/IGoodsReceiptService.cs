using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IGoodsReceiptService
{
    Task<List<GoodsReceiptDto>> GetAllAsync(int orgId);
    Task<GoodsReceiptDto?> GetByIdAsync(int id);
    Task<GoodsReceiptDto> CreateAsync(int orgId, CreateGoodsReceiptDto dto);
}
