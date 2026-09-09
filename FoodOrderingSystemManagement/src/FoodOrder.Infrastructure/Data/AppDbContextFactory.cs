using FoodOrder.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FoodOrder.Infrastructure.Data;

/// <summary>
/// Used exclusively by EF Core CLI tools (dotnet ef migrations add / update).
/// Not loaded at runtime — the real DbContext is registered in Program.cs via DI.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>No HTTP request exists at design time — query filters referencing this are inert for migrations.</summary>
    private class DesignTimeCurrentUserContext : ICurrentUserContext
    {
        public int? OrganizationId => null;
        public int? BranchId => null;
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "FoodOrder.API");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        return new AppDbContext(optionsBuilder.Options, new DesignTimeCurrentUserContext());
    }
}
