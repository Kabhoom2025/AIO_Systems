using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Application.Features.Auth;

public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetCurrentUserQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var (roles, permissions) = await UserAuthorizationHelper.GetRolesAndPermissionsAsync(_db, user.Id, cancellationToken);

        return new CurrentUserDto(user.Id, user.Email, user.FirstName, user.LastName, user.AvatarUrl,
            user.OrganizationId, roles, permissions);
    }
}
