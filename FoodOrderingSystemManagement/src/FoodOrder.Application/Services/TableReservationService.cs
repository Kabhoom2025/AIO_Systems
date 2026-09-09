using FoodOrder.Application.DTOs.Reservation;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Services;

public class TableReservationService : ITableReservationService
{
    private readonly ITableReservationRepository _repo;

    public TableReservationService(ITableReservationRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var list = await _repo.GetAllAsync(from, to);
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ReservationDto>> GetByTableAsync(int tableId)
    {
        var list = await _repo.GetByTableAsync(tableId);
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ReservationDto>> GetUpcomingAsync()
    {
        var list = await _repo.GetUpcomingAsync();
        return list.Select(Map).ToList();
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        return r is null ? null : Map(r);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationRequest req)
    {
        var reservation = new TableReservation
        {
            TableId             = req.TableId,
            GuestName           = req.GuestName.Trim(),
            GuestPhone          = req.GuestPhone.Trim(),
            PartySize           = req.PartySize,
            ReservationDateTime = DateTime.SpecifyKind(req.ReservationDateTime, DateTimeKind.Utc),
            Notes               = req.Notes?.Trim(),
            Status              = ReservationStatus.Pending,
            CreatedDate         = DateTime.UtcNow
        };

        await _repo.AddAsync(reservation);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(reservation.Id);
        return Map(created!);
    }

    public async Task<ReservationDto?> UpdateAsync(int id, UpdateReservationRequest req)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r is null) return null;

        r.GuestName           = req.GuestName.Trim();
        r.GuestPhone          = req.GuestPhone.Trim();
        r.PartySize           = req.PartySize;
        r.ReservationDateTime = DateTime.SpecifyKind(req.ReservationDateTime, DateTimeKind.Utc);
        r.Notes               = req.Notes?.Trim();

        await _repo.UpdateAsync(r);
        await _repo.SaveChangesAsync();
        return Map(r);
    }

    public async Task<ReservationDto?> UpdateStatusAsync(int id, string status)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r is null) return null;

        if (!Enum.TryParse<ReservationStatus>(status, ignoreCase: true, out var parsed))
            throw new InvalidOperationException($"Invalid reservation status '{status}'.");

        r.Status = parsed;
        await _repo.UpdateAsync(r);
        await _repo.SaveChangesAsync();
        return Map(r);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r is null) return false;

        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static ReservationDto Map(TableReservation r) => new()
    {
        Id                  = r.Id,
        TableId             = r.TableId,
        TableNumber         = r.Table?.TableNumber ?? 0,
        Hall                = r.Table?.Hall ?? string.Empty,
        GuestName           = r.GuestName,
        GuestPhone          = r.GuestPhone,
        PartySize           = r.PartySize,
        ReservationDateTime = r.ReservationDateTime,
        Notes               = r.Notes,
        Status              = r.Status.ToString(),
        CreatedDate         = r.CreatedDate
    };
}
