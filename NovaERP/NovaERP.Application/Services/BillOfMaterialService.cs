using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class BillOfMaterialService : IBillOfMaterialService
{
    private readonly IBillOfMaterialRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IValidator<CreateBillOfMaterialDto> _createValidator;
    private readonly IValidator<UpdateBillOfMaterialDto> _updateValidator;

    public BillOfMaterialService(IBillOfMaterialRepository repo, IProductRepository productRepo,
        IValidator<CreateBillOfMaterialDto> createValidator, IValidator<UpdateBillOfMaterialDto> updateValidator)
    {
        _repo = repo;
        _productRepo = productRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<BillOfMaterialDto>> GetAllAsync(int orgId)
    {
        var boms = await _repo.GetAllByOrgAsync(orgId);
        return boms.Select(ToDto).ToList();
    }

    public async Task<BillOfMaterialDto> GetByIdAsync(int orgId, int id)
    {
        var bom = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BillOfMaterial {id} not found");
        return ToDto(bom);
    }

    public async Task<BillOfMaterialDto> CreateAsync(int orgId, CreateBillOfMaterialDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var product = await _productRepo.GetByIdAsync(orgId, dto.ProductId)
            ?? throw new KeyNotFoundException($"Product {dto.ProductId} not found");

        if (await _repo.ExistsForProductAsync(orgId, dto.ProductId))
            throw new InvalidOperationException($"A bill of materials already exists for product {product.Name}");

        var bom = new BillOfMaterial
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            IsActive = dto.IsActive,
            Components = dto.Components.Select(c => new BomComponent
            {
                ComponentProductId = c.ComponentProductId,
                Quantity = c.Quantity,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        _repo.Add(bom);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bom.Id) ?? bom;
        return ToDto(reloaded);
    }

    public async Task<BillOfMaterialDto> UpdateAsync(int orgId, int id, UpdateBillOfMaterialDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var bom = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BillOfMaterial {id} not found");

        bom.IsActive = dto.IsActive;
        bom.UpdatedDate = DateTime.UtcNow;

        // Components are owned by the BOM and replaced wholesale on update.
        bom.Components.Clear();
        foreach (var c in dto.Components)
        {
            bom.Components.Add(new BomComponent
            {
                ComponentProductId = c.ComponentProductId,
                Quantity = c.Quantity,
                DisplayOrder = c.DisplayOrder
            });
        }

        _repo.Update(bom);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bom.Id) ?? bom;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var bom = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BillOfMaterial {id} not found");
        _repo.Remove(bom);
        await _repo.SaveChangesAsync();
    }

    private static BillOfMaterialDto ToDto(BillOfMaterial b) => new()
    {
        Id = b.Id,
        ProductId = b.ProductId,
        ProductName = b.Product?.Name ?? string.Empty,
        ProductSku = b.Product?.Sku ?? string.Empty,
        IsActive = b.IsActive,
        Components = b.Components.OrderBy(c => c.DisplayOrder).Select(c => new BomComponentDto
        {
            Id = c.Id,
            ComponentProductId = c.ComponentProductId,
            ComponentProductName = c.ComponentProduct?.Name ?? string.Empty,
            ComponentProductSku = c.ComponentProduct?.Sku ?? string.Empty,
            Quantity = c.Quantity,
            DisplayOrder = c.DisplayOrder
        }).ToList()
    };
}
