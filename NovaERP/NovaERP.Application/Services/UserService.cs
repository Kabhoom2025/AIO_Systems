using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly IValidator<ResetUserPasswordDto> _resetValidator;

    public UserService(IUserRepository repo, IMapper mapper, IPasswordHasher hasher,
        IValidator<CreateUserDto> createValidator, IValidator<UpdateUserDto> updateValidator,
        IValidator<ResetUserPasswordDto> resetValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _hasher = hasher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _resetValidator = resetValidator;
    }

    public async Task<List<UserDto>> GetAllAsync(int orgId)
    {
        var users = await _repo.GetAllByOrgAsync(orgId);
        return users.Select(_mapper.Map<UserDto>).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int orgId, int id)
    {
        var user = await GetOwnedUserOrNullAsync(orgId, id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(int orgId, CreateUserDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        if (await _repo.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");

        if (dto.PhotoData != null) ValidatePhotoSize(dto.PhotoData);

        var user = new User
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            Name           = dto.Name,
            Email          = dto.Email,
            PasswordHash   = _hasher.Hash(dto.Password),
            RoleId         = dto.RoleId,
            IsActive       = true,
            PhotoData        = dto.PhotoData,
            PhotoContentType = dto.PhotoData != null ? dto.PhotoContentType : null
        };

        _repo.Add(user);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(user.Id) ?? user;
        return _mapper.Map<UserDto>(created);
    }

    public async Task<UserDto> UpdateAsync(int orgId, int id, UpdateUserDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var user = await GetOwnedUserAsync(orgId, id);

        user.Name       = dto.Name;
        user.RoleId     = dto.RoleId;
        user.BranchId   = dto.BranchId;
        user.IsActive   = dto.IsActive;
        user.UpdatedDate = DateTime.UtcNow;

        if (dto.RemovePhoto)
        {
            user.PhotoData = null;
            user.PhotoContentType = null;
        }
        else if (dto.PhotoData != null)
        {
            ValidatePhotoSize(dto.PhotoData);
            user.PhotoData = dto.PhotoData;
            user.PhotoContentType = dto.PhotoContentType;
        }
        // else: PhotoData null and RemovePhoto false means "leave the existing photo unchanged".

        _repo.Update(user);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(user.Id) ?? user;
        return _mapper.Map<UserDto>(updated);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var user = await GetOwnedUserAsync(orgId, id);
        _repo.Remove(user);
        await _repo.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(int orgId, int id, ResetUserPasswordDto dto)
    {
        await _resetValidator.ValidateAndThrowAsync(dto);

        var user = await GetOwnedUserAsync(orgId, id);
        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
    }

    /// <summary>Base64 is ~1.37x the decoded size — 2,800,000 chars caps the decoded photo at
    /// roughly 2 MB, generous for a profile photo while keeping the row small.</summary>
    private static void ValidatePhotoSize(string photoDataBase64)
    {
        if (photoDataBase64.Length > 2_800_000)
            throw new InvalidOperationException("Photo is too large — please use an image under 2 MB.");
    }

    private async Task<User?> GetOwnedUserOrNullAsync(int orgId, int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user == null || user.OrganizationId != orgId ? null : user;
    }

    private async Task<User> GetOwnedUserAsync(int orgId, int id) =>
        await GetOwnedUserOrNullAsync(orgId, id) ?? throw new KeyNotFoundException($"User {id} not found");
}
