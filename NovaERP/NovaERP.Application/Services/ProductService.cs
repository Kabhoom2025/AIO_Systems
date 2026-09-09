using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductService(IProductRepository repo,
        IValidator<CreateProductDto> createValidator, IValidator<UpdateProductDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ProductDto>> GetAllAsync(int orgId)
    {
        var products = await _repo.GetAllByOrgAsync(orgId);
        var onHandByProduct = await _repo.GetOnHandQuantitiesByOrgAsync(orgId);
        return products.Select(p => ToDto(p, onHandByProduct.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(int orgId, int id)
    {
        var product = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Product {id} not found");
        var onHand = await _repo.GetOnHandQuantityAsync(orgId, id);
        return ToDto(product, onHand);
    }

    public async Task<ProductDto> CreateAsync(int orgId, CreateProductDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var sku = dto.Sku.ToUpperInvariant();
        if (await _repo.SkuExistsAsync(orgId, sku))
            throw new InvalidOperationException($"Product SKU {sku} already exists");

        var product = new Product
        {
            OrganizationId = orgId,
            Sku = sku,
            Name = dto.Name,
            Description = dto.Description,
            UnitOfMeasure = dto.UnitOfMeasure,
            UnitCost = dto.UnitCost,
            WeightKg = dto.WeightKg,
            IsActive = dto.IsActive
        };

        _repo.Add(product);
        await _repo.SaveChangesAsync();
        return ToDto(product, 0);
    }

    public async Task<ProductDto> UpdateAsync(int orgId, int id, UpdateProductDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var product = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.UnitOfMeasure = dto.UnitOfMeasure;
        product.UnitCost = dto.UnitCost;
        product.WeightKg = dto.WeightKg;
        product.IsActive = dto.IsActive;
        product.UpdatedDate = DateTime.UtcNow;

        _repo.Update(product);
        await _repo.SaveChangesAsync();

        var onHand = await _repo.GetOnHandQuantityAsync(orgId, id);
        return ToDto(product, onHand);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var product = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Product {id} not found");
        _repo.Remove(product);
        await _repo.SaveChangesAsync();
    }

    private static ProductDto ToDto(Product p, decimal onHandQuantity) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Description = p.Description,
        UnitOfMeasure = p.UnitOfMeasure,
        UnitCost = p.UnitCost,
        WeightKg = p.WeightKg,
        IsActive = p.IsActive,
        OnHandQuantity = onHandQuantity
    };
}
