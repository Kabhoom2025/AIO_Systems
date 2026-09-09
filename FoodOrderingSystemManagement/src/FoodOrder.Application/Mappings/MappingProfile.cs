using AutoMapper;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.DTOs.Category;
using FoodOrder.Application.DTOs.FoodItem;
using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.DTOs.Settings;
using FoodOrder.Application.DTOs.Table;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Mappings;

/// <summary>
/// Central AutoMapper profile. Add a new section per module as they are implemented.
/// Keep mappings grouped by entity so it is easy to find and extend them.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyCategoryMappings();
        ApplyFoodItemMappings();
        ApplyOrderMappings();
        ApplySettingsMappings();
        ApplyTableMappings();
        ApplyAddOnMappings();
        ApplyInventoryMappings();
        ApplyLedgerMappings();
        ApplyCustomerMappings();
        ApplySupplierMappings();
        ApplyOrganizationMappings();
    }

    private void ApplyCategoryMappings()
    {
        CreateMap<Category, CategoryDto>();

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.FoodItems, opt => opt.Ignore());

        // Used with _mapper.Map(dto, existingEntity) to update only supplied fields.
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.FoodItems, opt => opt.Ignore());
    }

    private void ApplyFoodItemMappings()
    {
        // CategoryName is flattened from the Category navigation property.
        CreateMap<FoodItem, FoodItemDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));

        CreateMap<CreateFoodItemDto, FoodItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

        CreateMap<UpdateFoodItemDto, FoodItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
    }

    private void ApplyOrderMappings()
    {
        // CashierName flattened from Cashier nav property; Status/OrderType enums stored as strings.
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CashierName,
                opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.Name : string.Empty))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.OrderType,
                opt => opt.MapFrom(src => src.OrderType.ToString()))
            .ForMember(dest => dest.TableNumber,
                opt => opt.MapFrom(src => src.Table != null ? (int?)src.Table.TableNumber : null))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.OrderItems));

        // UnitPrice mapped from Price (the price locked at order time, not the current food item price).
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ItemName,
                opt => opt.MapFrom(src => src.FoodItem != null ? src.FoodItem.ItemName : string.Empty))
            .ForMember(dest => dest.UnitPrice,
                opt => opt.MapFrom(src => src.Price));
    }

    private void ApplyTableMappings()
    {
        CreateMap<Domain.Entities.Table, TableDto>()
            .ForMember(dest => dest.IsOccupied, opt => opt.Ignore());
    }

    private void ApplySettingsMappings()
    {
        CreateMap<Settings, SettingsDto>();

        // Used with _mapper.Map(dto, existingSettings) — updates the single settings row in place.
        CreateMap<UpdateSettingsDto, Settings>()
            .ForMember(dest => dest.Id,             opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization,   opt => opt.Ignore());
    }

    private void ApplyAddOnMappings()
    {
        CreateMap<AddOn, AddOnDTO>();

        CreateMap<CreateAddOnRequest, AddOn>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.ItemAddOns, opt => opt.Ignore());

        CreateMap<UpdateAddOnRequest, AddOn>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ItemAddOns, opt => opt.Ignore());
    }

    private void ApplyLedgerMappings()
    {
        CreateMap<Domain.Entities.LedgerEntry, LedgerEntryDTO>()
            .ForMember(dest => dest.CreatedBy,
                opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Name : string.Empty))
            .ForMember(dest => dest.RunningBalance, opt => opt.Ignore());

        CreateMap<CreateLedgerEntryRequest, Domain.Entities.LedgerEntry>()
            .ForMember(dest => dest.Id,          opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy,   opt => opt.Ignore());
    }

    private void ApplyCustomerMappings()
    {
        CreateMap<Domain.Entities.Customer, DTOs.Customer.CustomerDto>()
            .ForMember(dest => dest.Tier, opt => opt.MapFrom(src => src.Tier));

        CreateMap<Domain.Entities.PointsTransaction, DTOs.Customer.PointsTransactionDto>();
    }

    private void ApplyInventoryMappings()
    {
        CreateMap<Domain.Entities.InventoryItem, InventoryItemDTO>()
            .ForMember(dest => dest.IsLowStock,
                opt => opt.MapFrom(src => src.CurrentStock <= src.MinimumStock));

        CreateMap<CreateInventoryItemRequest, Domain.Entities.InventoryItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.LastUpdated, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());

        CreateMap<UpdateInventoryItemRequest, Domain.Entities.InventoryItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdated, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());
    }

    private void ApplySupplierMappings()
    {
        CreateMap<Supplier, SupplierDTO>()
            .ForMember(dest => dest.PurchaseOrderCount, opt => opt.MapFrom(src => src.PurchaseOrders.Count))
            .ForMember(dest => dest.TotalPaid, opt => opt.MapFrom(src => src.Payments.Sum(p => p.Amount)));

        CreateMap<CreateSupplierRequest, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore());
    }

    private void ApplyOrganizationMappings()
    {
        CreateMap<Organization, OrganizationDTO>();
        CreateMap<CreateOrganizationRequest, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore());
        CreateMap<UpdateOrganizationRequest, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore());
    }
}
