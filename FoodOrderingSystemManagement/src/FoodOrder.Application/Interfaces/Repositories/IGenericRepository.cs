namespace FoodOrder.Application.Interfaces.Repositories;

/// <summary>
/// Generic CRUD contract shared by all repositories.
/// Keeps the application layer decoupled from EF Core — services depend on this
/// interface, not on DbContext directly.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync();
}
