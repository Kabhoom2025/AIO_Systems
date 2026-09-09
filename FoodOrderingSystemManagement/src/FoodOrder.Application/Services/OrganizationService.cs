using AutoMapper;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class OrganizationService(
    IOrganizationRepository repo,
    IBranchRepository branchRepository,
    ISettingsRepository settingsRepository,
    ILicenseRepository licenseRepository,
    IMapper mapper,
    IPasswordService passwordSvc) : IOrganizationService
{
    public async Task<IEnumerable<OrganizationDTO>> GetAllAsync()
    {
        var orgs = await repo.GetAllAsync();
        var dtos = new List<OrganizationDTO>();
        foreach (var org in orgs)
        {
            var userCount = await repo.GetUserCountAsync(org.Id);
            dtos.Add(new OrganizationDTO
            {
                Id = org.Id, Name = org.Name, Address = org.Address, Phone = org.Phone,
                Email = org.Email, LogoUrl = org.LogoUrl, IsActive = org.IsActive,
                Timezone = org.Timezone, Currency = org.Currency,
                UserCount = userCount, CreatedDate = org.CreatedDate,
            });
        }
        return dtos;
    }

    public async Task<OrganizationDTO?> GetByIdAsync(int id)
    {
        var org = await repo.GetByIdAsync(id);
        if (org == null) return null;
        var userCount = await repo.GetUserCountAsync(id);
        return new OrganizationDTO
        {
            Id = org.Id, Name = org.Name, Address = org.Address, Phone = org.Phone,
            Email = org.Email, LogoUrl = org.LogoUrl, IsActive = org.IsActive,
            Timezone = org.Timezone, Currency = org.Currency,
            UserCount = userCount, CreatedDate = org.CreatedDate,
        };
    }

    public async Task<OrganizationDTO> CreateAsync(CreateOrganizationRequest request)
    {
        var org = mapper.Map<Organization>(request);
        var created = await repo.CreateAsync(org);

        // Every organization needs at least one branch to assign menu/tables/orders
        // to — auto-create it so there's never a gap between "org exists" and
        // "org has somewhere to put data."
        await branchRepository.CreateAsync(new Branch
        {
            OrganizationId = created.Id,
            Name = $"{created.Name} - Main Branch",
            Address = created.Address,
            Phone = created.Phone,
            IsActive = true,
            IsDefault = true,
        });

        // Same reasoning as the default branch — without this, a new org has no
        // Settings row and GetSettings() would 404 until someone configures one.
        await settingsRepository.AddAsync(new Settings
        {
            OrganizationId = created.Id,
            RestaurantName = created.Name,
            Address = created.Address,
            Phone = created.Phone,
        });
        await settingsRepository.SaveChangesAsync();

        // Without a License row, LicenseMiddleware blocks every login for this org's
        // users — a new organization must start on a Trial so its first admin can
        // actually sign in.
        await licenseRepository.UpsertAsync(created.Id, new UpsertLicenseRequest
        {
            OrganizationId = created.Id,
            Plan = "Basic",
            Status = "Trial",
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            MaxUsers = 5,
            Notes = "Auto-provisioned trial license.",
        });

        return await GetByIdAsync(created.Id) ?? throw new Exception("Organization not found after create");
    }

    public async Task<OrganizationDTO> UpdateAsync(int id, UpdateOrganizationRequest request)
    {
        var org = await repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Organization {id} not found");
        mapper.Map(request, org);
        await repo.UpdateAsync(org);
        return await GetByIdAsync(id) ?? throw new Exception("Organization not found after update");
    }

    public async Task DeleteAsync(int id)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Organization {id} not found");

        // Once an org has real users, it also has real menu/order data tied to its
        // branches — cascading through all of that is a much bigger operation than
        // "delete an empty shell." Deactivate instead of hard-deleting in that case.
        if (await repo.GetUserCountAsync(id) > 0)
            throw new AppException("Cannot delete an organization that has users. Deactivate it instead.", 400);

        var settings = await settingsRepository.GetByOrgAsync(id);
        if (settings != null)
        {
            settingsRepository.Delete(settings);
            await settingsRepository.SaveChangesAsync();
        }

        await licenseRepository.DeleteAsync(id);

        var branches = await branchRepository.GetByOrganizationAsync(id);
        foreach (var branch in branches)
            await branchRepository.DeleteAsync(branch.Id);

        await repo.DeleteAsync(id);
    }

    public async Task<IEnumerable<OrgUserDTO>> GetOrgUsersAsync(int organizationId)
    {
        var users = await repo.GetOrgUsersAsync(organizationId);
        return users.Select(u => new OrgUserDTO
        {
            Id = u.Id, Name = u.Name, Email = u.Email,
            RoleName = u.Role.RoleName,
            BranchId = u.BranchId, BranchName = u.Branch?.Name,
            IsActive = u.IsActive, CreatedDate = u.CreatedDate,
        });
    }

    public async Task<OrgReportDto> GetOrgReportAsync(int orgId)
    {
        var org = await repo.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException($"Organization {orgId} not found");
        var report = await repo.GetOrgReportAsync(orgId);
        report.OrgName = org.Name;
        return report;
    }

    public async Task<OrgUserDTO> CreateOrgAdminAsync(int organizationId, CreateOrgAdminRequest request)
    {
        var org = await repo.GetByIdAsync(organizationId)
            ?? throw new KeyNotFoundException($"Organization {organizationId} not found");

        if (await repo.EmailExistsAsync(request.Email))
            throw new AppException("A user with this email already exists.", statusCode: 409);

        if (request.BranchId.HasValue)
        {
            var branch = await branchRepository.GetByIdAsync(request.BranchId.Value)
                ?? throw new AppException("Branch not found.", 404);
            if (branch.OrganizationId != organizationId)
                throw new AppException("That branch does not belong to this organization.", 400);
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordSvc.HashPassword(request.Password),
            RoleId = 2,  // Admin role
            OrganizationId = organizationId,
            BranchId = request.BranchId,
            IsActive = true,
        };

        var created = await repo.CreateUserAsync(user);
        return new OrgUserDTO
        {
            Id = created.Id, Name = created.Name, Email = created.Email,
            RoleName = created.Role.RoleName,
            BranchId = created.BranchId, BranchName = created.Branch?.Name,
            IsActive = created.IsActive, CreatedDate = created.CreatedDate,
        };
    }
}
