using FoodOrder.Application.DTOs.Reservation;

namespace FoodOrder.Application.Interfaces.Services;

public interface ITableReservationService
{
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<ReservationDto>> GetByTableAsync(int tableId);
    Task<IReadOnlyList<ReservationDto>> GetUpcomingAsync();
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto>  CreateAsync(CreateReservationRequest request);
    Task<ReservationDto?> UpdateAsync(int id, UpdateReservationRequest request);
    Task<ReservationDto?> UpdateStatusAsync(int id, string status);
    Task<bool>            DeleteAsync(int id);
}
