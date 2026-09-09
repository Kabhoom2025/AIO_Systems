using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Infrastructure.Data.SeedData;

/// <summary>
/// Runs pending EF Core migrations and seeds essential reference data at application startup.
/// Roles and Settings are seeded via HasData in entity configurations.
/// The default Admin user is seeded here because it requires BCrypt hashing at runtime.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            await SeedAdminUserAsync(context, logger);
            await SeedInventoryItemsAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedAdminUserAsync(AppDbContext context, ILogger logger)
    {
        var passwordService = new PasswordService();

        // ── Fix: if admin@foodorder.com was accidentally given RoleId=1 (SuperAdmin) ──
        // This happened when the multi-tenancy migration shifted Role IDs.
        var misassignedAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "admin@foodorder.com" && u.RoleId == 1);
        if (misassignedAdmin != null)
        {
            misassignedAdmin.RoleId = 2; // Correct to Admin
            await context.SaveChangesAsync();
            logger.LogInformation("Fixed admin@foodorder.com: role corrected from SuperAdmin to Admin.");
        }

        // ── Ensure a SuperAdmin user exists (RoleId=1, no OrganizationId) ──────────
        var superAdminExists = await context.Users
            .AnyAsync(u => u.RoleId == 1 && u.OrganizationId == null);
        if (!superAdminExists)
        {
            context.Users.Add(new User
            {
                Name = "Super Admin",
                Email = "superadmin@foodorder.com",
                PasswordHash = passwordService.HashPassword("SuperAdmin@123"),
                RoleId = 1,
                OrganizationId = null,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
            logger.LogInformation("SuperAdmin seeded — Email: superadmin@foodorder.com | Password: SuperAdmin@123");
            logger.LogWarning("IMPORTANT: Change the SuperAdmin password after first login.");
        }

        // ── Ensure a restaurant Admin user exists (RoleId=2) ──────────────────────
        var restaurantAdminExists = await context.Users.AnyAsync(u => u.RoleId == 2);
        if (!restaurantAdminExists)
        {
            context.Users.Add(new User
            {
                Name = "System Admin",
                Email = "admin@foodorder.com",
                PasswordHash = passwordService.HashPassword("Admin@123"),
                RoleId = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
            logger.LogInformation("Restaurant admin seeded — Email: admin@foodorder.com | Password: Admin@123");
            logger.LogWarning("IMPORTANT: Change the default admin password after first login.");
        }
    }

    private static async Task SeedInventoryItemsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.InventoryItems.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        const int defaultBranchId = 1;

        var items = new List<InventoryItem>
        {
            // ── Grains & Staples ──────────────────────────────────────────────
            new() { Name = "Basmati Rice",       Description = "Long-grain rice for biryani & fried rice", Unit = "kg",     CurrentStock = 50,   MinimumStock = 10,  IsActive = true, LastUpdated = now },
            new() { Name = "Wheat Flour (Maida)", Description = "All-purpose flour for dough & batter",   Unit = "kg",     CurrentStock = 20,   MinimumStock = 5,   IsActive = true, LastUpdated = now },
            new() { Name = "Bread Buns",          Description = "Burger & sandwich buns",                 Unit = "pieces", CurrentStock = 80,   MinimumStock = 20,  IsActive = true, LastUpdated = now },

            // ── Proteins ──────────────────────────────────────────────────────
            new() { Name = "Chicken (Whole)",     Description = "Fresh whole chicken",                    Unit = "kg",     CurrentStock = 15,   MinimumStock = 5,   IsActive = true, LastUpdated = now },
            new() { Name = "Chicken Breast",      Description = "Boneless chicken breast fillet",         Unit = "kg",     CurrentStock = 10,   MinimumStock = 4,   IsActive = true, LastUpdated = now },
            new() { Name = "Mutton",              Description = "Fresh mutton/lamb pieces",               Unit = "kg",     CurrentStock = 8,    MinimumStock = 3,   IsActive = true, LastUpdated = now },
            new() { Name = "Paneer",              Description = "Fresh cottage cheese",                   Unit = "kg",     CurrentStock = 5,    MinimumStock = 2,   IsActive = true, LastUpdated = now },
            new() { Name = "Eggs",                Description = "Farm fresh eggs",                        Unit = "pieces", CurrentStock = 120,  MinimumStock = 30,  IsActive = true, LastUpdated = now },

            // ── Dairy ─────────────────────────────────────────────────────────
            new() { Name = "Milk",                Description = "Full-fat fresh milk",                    Unit = "liters", CurrentStock = 10,   MinimumStock = 3,   IsActive = true, LastUpdated = now },
            new() { Name = "Butter",              Description = "Salted table butter",                    Unit = "kg",     CurrentStock = 3,    MinimumStock = 1,   IsActive = true, LastUpdated = now },
            new() { Name = "Cheese (Mozzarella)", Description = "Shredded mozzarella for pizza & burgers",Unit = "kg",     CurrentStock = 4,    MinimumStock = 1,   IsActive = true, LastUpdated = now },
            new() { Name = "Fresh Cream",         Description = "Heavy cream for gravies & desserts",     Unit = "liters", CurrentStock = 2,    MinimumStock = 1,   IsActive = true, LastUpdated = now },

            // ── Vegetables ────────────────────────────────────────────────────
            new() { Name = "Onions",              Description = "Red onions",                             Unit = "kg",     CurrentStock = 20,   MinimumStock = 5,   IsActive = true, LastUpdated = now },
            new() { Name = "Tomatoes",            Description = "Fresh red tomatoes",                     Unit = "kg",     CurrentStock = 10,   MinimumStock = 4,   IsActive = true, LastUpdated = now },
            new() { Name = "Potatoes",            Description = "Fresh potatoes",                         Unit = "kg",     CurrentStock = 15,   MinimumStock = 5,   IsActive = true, LastUpdated = now },
            new() { Name = "Garlic",              Description = "Fresh garlic pods",                      Unit = "kg",     CurrentStock = 3,    MinimumStock = 1,   IsActive = true, LastUpdated = now },
            new() { Name = "Ginger",              Description = "Fresh ginger root",                      Unit = "kg",     CurrentStock = 2,    MinimumStock = 0.5m,IsActive = true, LastUpdated = now },
            new() { Name = "Green Chilies",       Description = "Fresh green chili peppers",              Unit = "kg",     CurrentStock = 1,    MinimumStock = 0.5m,IsActive = true, LastUpdated = now },
            new() { Name = "Capsicum",            Description = "Bell peppers (mixed colours)",           Unit = "kg",     CurrentStock = 3,    MinimumStock = 1,   IsActive = true, LastUpdated = now },

            // ── Spices & Powders ──────────────────────────────────────────────
            new() { Name = "Red Chili Powder",    Description = "Ground red chili",                       Unit = "grams",  CurrentStock = 1000, MinimumStock = 200, IsActive = true, LastUpdated = now },
            new() { Name = "Turmeric Powder",     Description = "Ground turmeric",                        Unit = "grams",  CurrentStock = 500,  MinimumStock = 100, IsActive = true, LastUpdated = now },
            new() { Name = "Garam Masala",        Description = "Whole spice blend",                      Unit = "grams",  CurrentStock = 400,  MinimumStock = 100, IsActive = true, LastUpdated = now },
            new() { Name = "Coriander Powder",    Description = "Ground coriander seeds",                 Unit = "grams",  CurrentStock = 600,  MinimumStock = 150, IsActive = true, LastUpdated = now },
            new() { Name = "Cumin Seeds",         Description = "Whole jeera",                            Unit = "grams",  CurrentStock = 300,  MinimumStock = 100, IsActive = true, LastUpdated = now },
            new() { Name = "Salt",                Description = "Table salt",                             Unit = "kg",     CurrentStock = 5,    MinimumStock = 1,   IsActive = true, LastUpdated = now },

            // ── Oils & Condiments ─────────────────────────────────────────────
            new() { Name = "Cooking Oil",         Description = "Sunflower / refined oil",                Unit = "liters", CurrentStock = 15,   MinimumStock = 5,   IsActive = true, LastUpdated = now },
            new() { Name = "Tomato Ketchup",      Description = "Bottled ketchup",                        Unit = "liters", CurrentStock = 4,    MinimumStock = 1,   IsActive = true, LastUpdated = now },
            new() { Name = "Mayonnaise",          Description = "Eggless mayo",                           Unit = "kg",     CurrentStock = 2,    MinimumStock = 0.5m,IsActive = true, LastUpdated = now },

            // ── Beverages ─────────────────────────────────────────────────────
            new() { Name = "Cold Drink Cans",     Description = "Assorted 330ml cans",                    Unit = "pieces", CurrentStock = 48,   MinimumStock = 12,  IsActive = true, LastUpdated = now },
            new() { Name = "Mineral Water Bottles",Description = "500ml sealed bottles",                  Unit = "pieces", CurrentStock = 60,   MinimumStock = 24,  IsActive = true, LastUpdated = now },
            new() { Name = "Tea Leaves",          Description = "CTC tea",                                Unit = "grams",  CurrentStock = 500,  MinimumStock = 100, IsActive = true, LastUpdated = now },
            new() { Name = "Coffee Powder",       Description = "Filter coffee blend",                    Unit = "grams",  CurrentStock = 300,  MinimumStock = 100, IsActive = true, LastUpdated = now },

            // ── Packaging ─────────────────────────────────────────────────────
            new() { Name = "Takeaway Boxes",      Description = "Disposable food boxes",                  Unit = "pieces", CurrentStock = 200,  MinimumStock = 50,  IsActive = true, LastUpdated = now },
            new() { Name = "Tissue Paper Rolls",  Description = "Napkin tissue rolls",                    Unit = "pieces", CurrentStock = 30,   MinimumStock = 10,  IsActive = true, LastUpdated = now },
        };

        foreach (var item in items)
            item.BranchId = defaultBranchId;

        await context.InventoryItems.AddRangeAsync(items);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} inventory items.", items.Count);
    }
}
