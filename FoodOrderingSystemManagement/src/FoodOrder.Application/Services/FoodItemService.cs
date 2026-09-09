using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.FoodItem;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class FoodItemService : IFoodItemService
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public FoodItemService(
        IFoodItemRepository foodItemRepository,
        ICategoryRepository categoryRepository,
        IBranchRepository branchRepository,
        ICurrentUserContext currentUser,
        IMapper mapper)
    {
        _foodItemRepository = foodItemRepository;
        _categoryRepository = categoryRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<FoodItemDto>> GetAllAsync()
    {
        var items = await _foodItemRepository.GetAllWithCategoryAsync();
        return _mapper.Map<IReadOnlyList<FoodItemDto>>(items);
    }

    public async Task<IReadOnlyList<FoodItemDto>> GetAllByBranchAsync(int branchId)
    {
        var items = await _foodItemRepository.GetAllWithCategoryByBranchAsync(branchId);
        return _mapper.Map<IReadOnlyList<FoodItemDto>>(items);
    }

    public async Task<IReadOnlyList<FoodItemDto>> GetByCategoryAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId)
            ?? throw new NotFoundException("Category", categoryId);

        if (!category.IsActive)
            throw new AppException("This category is not active.", statusCode: 400);

        var items = await _foodItemRepository.GetByCategoryAsync(categoryId);
        return _mapper.Map<IReadOnlyList<FoodItemDto>>(items);
    }

    public async Task<FoodItemDto> GetByIdAsync(int id)
    {
        var item = await _foodItemRepository.GetByIdWithCategoryAsync(id)
            ?? throw new NotFoundException("Food item", id);

        return _mapper.Map<FoodItemDto>(item);
    }

    public async Task<FoodItemDto> CreateAsync(CreateFoodItemDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException("Category", dto.CategoryId);

        if (!category.IsActive)
            throw new AppException("Cannot add items to an inactive category.");

        if (await _foodItemRepository.ExistsByNameInCategoryAsync(dto.ItemName, dto.CategoryId))
            throw new AppException($"A food item named '{dto.ItemName}' already exists in this category.");

        var foodItem = _mapper.Map<FoodItem>(dto);
        foodItem.BranchId = await TenantResolution.ResolveWriteBranchIdAsync(_currentUser, _branchRepository);
        await _foodItemRepository.AddAsync(foodItem);
        await _foodItemRepository.SaveChangesAsync();

        // Reload with Category populated so CategoryName is present in the response.
        var created = await _foodItemRepository.GetByIdWithCategoryAsync(foodItem.Id);
        return _mapper.Map<FoodItemDto>(created!);
    }

    public async Task<FoodItemDto> UpdateAsync(int id, UpdateFoodItemDto dto)
    {
        var foodItem = await _foodItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Food item", id);

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException("Category", dto.CategoryId);

        if (!category.IsActive)
            throw new AppException("Cannot move items to an inactive category.");

        if (await _foodItemRepository.ExistsByNameInCategoryAsync(dto.ItemName, dto.CategoryId, excludeId: id))
            throw new AppException($"A food item named '{dto.ItemName}' already exists in this category.");

        _mapper.Map(dto, foodItem);
        _foodItemRepository.Update(foodItem);
        await _foodItemRepository.SaveChangesAsync();

        var updated = await _foodItemRepository.GetByIdWithCategoryAsync(id);
        return _mapper.Map<FoodItemDto>(updated!);
    }

    public async Task DeleteAsync(int id)
    {
        var foodItem = await _foodItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Food item", id);

        // Soft delete — IsAvailable = false keeps the item in order history intact.
        foodItem.IsAvailable = false;
        _foodItemRepository.Update(foodItem);
        await _foodItemRepository.SaveChangesAsync();
    }

    public async Task<FoodItemDto?> GetByBarcodeAsync(string barcode)
    {
        var item = await _foodItemRepository.GetByBarcodeAsync(barcode.Trim());
        return item is null ? null : _mapper.Map<FoodItemDto>(item);
    }
}
