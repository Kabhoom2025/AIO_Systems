using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Table;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public TableService(
        ITableRepository tableRepository,
        IBranchRepository branchRepository,
        ICurrentUserContext currentUser,
        IMapper mapper)
    {
        _tableRepository = tableRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TableDto>> GetAllAsync()
    {
        var tables = await _tableRepository.GetAllWithStatusAsync();
        return tables.Select(t => MapWithStatus(t)).ToList().AsReadOnly();
    }

    public async Task<TableDto> GetByIdAsync(int id)
    {
        var table = await _tableRepository.GetAllWithStatusAsync()
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == id));
        if (table == null) throw new NotFoundException("Table", id);
        return MapWithStatus(table);
    }

    public async Task<TableDto> CreateAsync(CreateTableDto dto)
    {
        var hall = string.IsNullOrWhiteSpace(dto.Hall) ? "AC" : dto.Hall;
        if (await _tableRepository.ExistsByNumberAsync(dto.TableNumber, hall))
            throw new AppException($"Table {dto.TableNumber} already exists in the {hall} hall.");

        var table = new Table
        {
            TableNumber = dto.TableNumber,
            Capacity    = dto.Capacity,
            Hall        = hall,
            BranchId    = await TenantResolution.ResolveWriteBranchIdAsync(_currentUser, _branchRepository),
        };

        await _tableRepository.AddAsync(table);
        await _tableRepository.SaveChangesAsync();
        return MapWithStatus(table);
    }

    public async Task<TableDto> UpdateAsync(int id, UpdateTableDto dto)
    {
        var table = await _tableRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Table", id);

        var hall = string.IsNullOrWhiteSpace(dto.Hall) ? table.Hall : dto.Hall;
        if (await _tableRepository.ExistsByNumberAsync(dto.TableNumber, hall, excludeId: id))
            throw new AppException($"Table {dto.TableNumber} already exists in the {hall} hall.");

        table.TableNumber = dto.TableNumber;
        table.Capacity    = dto.Capacity;
        table.Hall        = hall;
        table.IsActive    = dto.IsActive;

        _tableRepository.Update(table);
        await _tableRepository.SaveChangesAsync();
        return MapWithStatus(table);
    }

    public async Task DeleteAsync(int id)
    {
        var table = await _tableRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Table", id);

        table.IsActive = false;
        _tableRepository.Update(table);
        await _tableRepository.SaveChangesAsync();
    }

    private static TableDto MapWithStatus(Table table)
    {
        var isOccupied = table.Orders.Any(o =>
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.Completed);

        var billPending = table.Orders.Any(o => o.Status == OrderStatus.BillPending);

        return new TableDto
        {
            Id          = table.Id,
            TableNumber = table.TableNumber,
            Capacity    = table.Capacity,
            Hall        = table.Hall,
            IsActive    = table.IsActive,
            IsOccupied  = isOccupied,
            BillPending = billPending,
        };
    }
}
