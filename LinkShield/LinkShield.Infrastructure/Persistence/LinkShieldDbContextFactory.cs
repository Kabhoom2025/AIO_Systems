using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinkShield.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add` run against LinkShield.Infrastructure directly,
/// without needing the API project's full DI container spun up.
/// </summary>
public class LinkShieldDbContextFactory : IDesignTimeDbContextFactory<LinkShieldDbContext>
{
    public LinkShieldDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LinkShieldDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LinkShieldDB;Username=postgres;Password=1234");
        return new LinkShieldDbContext(optionsBuilder.Options);
    }
}
