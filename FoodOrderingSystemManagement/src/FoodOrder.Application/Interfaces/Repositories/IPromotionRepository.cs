using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> GetAllAsync();
    Task<Promotion?> GetByIdAsync(int id);
    Task<Promotion?> GetByCodeAsync(string code);
    Task<IReadOnlyList<Promotion>> GetActiveAsync();
    /// <summary>Bypasses tenant query filters — for the anonymous public dashboard slider, which has no JWT claims.</summary>
    Task<IReadOnlyList<Promotion>> GetPublicActiveAsync(int organizationId);
    Task AddAsync(Promotion promotion);
    void Update(Promotion promotion);
    void Delete(Promotion promotion);
    Task SaveChangesAsync();
}
