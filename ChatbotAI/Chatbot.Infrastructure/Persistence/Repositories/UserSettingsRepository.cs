using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Infrastructure.Persistence.Repositories;

public class UserSettingsRepository(ApplicationDbContext context) : IUserSettingsRepository
{
    public Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

    public async Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default) =>
        await context.UserSettings.AddAsync(settings, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
