using AutoMapper;
using NovaERP.Application.DTOs;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Mapping;

/// <summary>Single AutoMapper profile covering every foundation entity <-> DTO mapping.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Organization, OrganizationDto>();
        CreateMap<UpdateOrganizationDto, Organization>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<Branch, BranchDto>();
        CreateMap<CreateBranchDto, Branch>();
        CreateMap<UpdateBranchDto, Branch>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<Department, DepartmentDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.ParentName, o => o.MapFrom(s => s.Parent != null ? s.Parent.Name : null));
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<Role, RoleDto>()
            .ForMember(d => d.PermissionKeys, o => o.MapFrom(s =>
                s.RolePermissions.Where(rp => rp.Permission != null)
                    .Select(rp => rp.Permission.Key).OrderBy(k => k).ToList()))
            .ForMember(d => d.UserCount, o => o.MapFrom(s => s.Users.Count));

        CreateMap<Permission, PermissionDto>();

        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.PhotoUrl, o => o.MapFrom(s =>
                s.PhotoData != null && s.PhotoContentType != null
                    ? $"data:{s.PhotoContentType};base64,{s.PhotoData}"
                    : null));

        CreateMap<Notification, NotificationDto>();

        CreateMap<AuditLog, AuditLogDto>();

        CreateMap<OrganizationSettings, OrganizationSettingsDto>();
        CreateMap<UpdateOrganizationSettingsDto, OrganizationSettings>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizationId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<FeatureToggle, FeatureToggleDto>();

        CreateMap<Currency, CurrencyDto>();
        CreateMap<CreateCurrencyDto, Currency>();
        CreateMap<UpdateCurrencyDto, Currency>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<ExchangeRate, ExchangeRateDto>();
        CreateMap<CreateExchangeRateDto, ExchangeRate>();
        CreateMap<UpdateExchangeRateDto, ExchangeRate>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizationId, o => o.Ignore())
            .ForMember(d => d.FromCurrencyCode, o => o.Ignore())
            .ForMember(d => d.ToCurrencyCode, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<Language, LanguageDto>();

        CreateMap<Document, DocumentDto>();

        CreateMap<AutomationRule, AutomationRuleDto>()
            .ForMember(d => d.NotifyRoleName, o => o.MapFrom(s => s.NotifyRole != null ? s.NotifyRole.Name : null));
        CreateMap<CreateAutomationRuleDto, AutomationRule>();
        CreateMap<UpdateAutomationRuleDto, AutomationRule>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizationId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());

        CreateMap<ScheduledJobDefinition, ScheduledJobDefinitionDto>();

        CreateMap<NotificationChannelSettings, NotificationChannelSettingsDto>();
        CreateMap<UpdateNotificationChannelSettingsDto, NotificationChannelSettings>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizationId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore());
    }
}
