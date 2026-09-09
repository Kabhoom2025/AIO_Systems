using FoodOrder.Application.DTOs.Promotion;

namespace FoodOrder.Application.Interfaces.Services;

public interface IPromotionService
{
    Task<IReadOnlyList<PromotionDto>> GetAllAsync();
    Task<PromotionDto> GetByIdAsync(int id);
    Task<IReadOnlyList<PromotionDto>> GetActiveAsync();
    Task<IReadOnlyList<PromotionDto>> GetPublicActiveAsync(int organizationId);
    Task<PromotionDto> CreateAsync(CreatePromotionDto dto);
    Task<PromotionDto> UpdateAsync(int id, CreatePromotionDto dto);
    Task DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
    Task<ApplyPromotionResultDto> ApplyCodeAsync(ApplyPromoCodeDto dto);
}
