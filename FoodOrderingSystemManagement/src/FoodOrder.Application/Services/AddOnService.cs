using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class AddOnService : IAddOnService
{
    private readonly IAddOnRepository _addOnRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public AddOnService(IAddOnRepository addOnRepository, ICurrentUserContext currentUser, IMapper mapper)
    {
        _addOnRepository = addOnRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AddOnDTO>> GetAllAsync()
    {
        var addOns = await _addOnRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<AddOnDTO>>(addOns);
    }

    public async Task<IReadOnlyList<AddOnDTO>> GetByFoodItemIdAsync(int foodItemId)
    {
        var addOns = await _addOnRepository.GetByFoodItemIdAsync(foodItemId);
        return _mapper.Map<IReadOnlyList<AddOnDTO>>(addOns);
    }

    public async Task<AddOnDTO> GetByIdAsync(int id)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("AddOn", id);

        return _mapper.Map<AddOnDTO>(addOn);
    }

    public async Task<AddOnDTO> CreateAsync(CreateAddOnRequest dto)
    {
        var addOn = _mapper.Map<Domain.Entities.AddOn>(dto);
        addOn.OrganizationId = TenantResolution.ResolveWriteOrganizationId(_currentUser);
        await _addOnRepository.AddAsync(addOn);
        await _addOnRepository.SaveChangesAsync();

        return _mapper.Map<AddOnDTO>(addOn);
    }

    public async Task<AddOnDTO> UpdateAsync(int id, UpdateAddOnRequest dto)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("AddOn", id);

        _mapper.Map(dto, addOn);
        await _addOnRepository.UpdateAsync(addOn);
        await _addOnRepository.SaveChangesAsync();

        return _mapper.Map<AddOnDTO>(addOn);
    }

    public async Task DeleteAsync(int id)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("AddOn", id);

        await _addOnRepository.DeleteAsync(id);
        await _addOnRepository.SaveChangesAsync();
    }
}
