using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Category;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IBranchRepository branchRepository,
        ICurrentUserContext currentUser,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllActiveAsync();
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllByBranchAsync(int branchId)
    {
        var categories = await _categoryRepository.GetAllActiveByBranchAsync(branchId);
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.ExistsByNameAsync(dto.CategoryName))
            throw new AppException($"A category named '{dto.CategoryName}' already exists.");

        var category = _mapper.Map<Category>(dto);
        category.BranchId = await TenantResolution.ResolveWriteBranchIdAsync(_currentUser, _branchRepository);
        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        if (await _categoryRepository.ExistsByNameAsync(dto.CategoryName, excludeId: id))
            throw new AppException($"A category named '{dto.CategoryName}' already exists.");

        _mapper.Map(dto, category);
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        // Soft delete — preserves food items and order history linked to this category.
        category.IsActive = false;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
    }
}
