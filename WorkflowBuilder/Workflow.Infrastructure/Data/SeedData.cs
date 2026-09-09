using Microsoft.EntityFrameworkCore;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Authentication;

namespace Workflow.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(WorkflowDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher();
        db.Users.Add(new User
        {
            OrganizationId = 1,
            Name           = "Workflow Admin",
            Email          = "admin@workflowbuilder.local",
            PasswordHash   = hasher.Hash("Admin@123"),
            Role           = "Admin",
            IsActive       = true
        });

        await db.SaveChangesAsync();
    }
}
