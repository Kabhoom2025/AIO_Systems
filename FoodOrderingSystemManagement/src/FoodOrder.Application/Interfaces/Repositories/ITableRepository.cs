using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ITableRepository : IGenericRepository<Table>
{
    Task<IReadOnlyList<Table>> GetAllWithStatusAsync();
    Task<bool> ExistsByNumberAsync(int tableNumber, string hall, int? excludeId = null);

    /// <summary>Bypasses tenant query filters — for the anonymous public order endpoint, which has no JWT claims.</summary>
    Task<Table?> GetByIdForBranchAsync(int id, int branchId);
}
