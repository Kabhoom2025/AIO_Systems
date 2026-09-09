using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IHolidayRepository
{
    Task<List<Holiday>> GetAllByOrgAsync(int orgId, int year);
    Task<List<Holiday>> GetUpcomingAsync(int orgId, DateOnly fromDate, int count);
    Task<Holiday?> GetByIdAsync(int orgId, int id);
    void Add(Holiday holiday);
    void Update(Holiday holiday);
    void Remove(Holiday holiday);
    Task SaveChangesAsync();
}
