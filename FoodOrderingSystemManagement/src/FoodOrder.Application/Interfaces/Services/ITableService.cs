using FoodOrder.Application.DTOs.Table;

namespace FoodOrder.Application.Interfaces.Services;

public interface ITableService
{
    Task<IReadOnlyList<TableDto>> GetAllAsync();
    Task<TableDto> GetByIdAsync(int id);
    Task<TableDto> CreateAsync(CreateTableDto dto);
    Task<TableDto> UpdateAsync(int id, UpdateTableDto dto);
    Task DeleteAsync(int id);
}
