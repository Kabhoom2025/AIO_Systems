using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.Auth;
using Platform.Application.DataDesigner;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Seeds the spec's own "Customer Management" demo (screen, table, API, mappings) plus a demo
/// login, so a fresh clone has a working example to run without clicking through every designer
/// (or standing up its own user) by hand first. Built entirely through the same application
/// services the designers use - never raw EF Core - so it's exercised through, and stays in sync
/// with, the same validation every real user goes through. Each half is independently
/// idempotent: skipped if an application with this name, or a user with this email, already
/// exists - so re-running never fails or duplicates either one.
/// </summary>
public static class SeedData
{
    public const string ApplicationName = "Customer Management";
    public const string DemoUserEmail = "admin@lineagestudio.local";
    public const string DemoUserPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<PlatformDbContext>();

        await SeedDemoUserAsync(db, services, ct);

        if (await db.Applications.AnyAsync(a => a.Name == ApplicationName, ct))
            return;

        var applications = services.GetRequiredService<IApplicationService>();
        var screens = services.GetRequiredService<IScreenService>();
        var components = services.GetRequiredService<IComponentService>();
        var tables = services.GetRequiredService<IDataTableService>();
        var serviceDefinitions = services.GetRequiredService<IServiceDefinitionService>();
        var apis = services.GetRequiredService<IApiEndpointService>();
        var mappings = services.GetRequiredService<IMappingService>();

        var app = await applications.CreateAsync(
            new CreateApplicationRequest(ApplicationName, "Demo: register a customer and write them to a real PostgreSQL table."), ct);

        var screen = await screens.CreateAsync(app.Id,
            new CreateScreenRequest("Customer Registration", "/customer-registration"), ct);

        var table = await tables.CreateAsync(app.Id, new CreateTableRequest("customer",
        [
            new ColumnDefinition("id", ColumnDataType.Uuid, IsPrimaryKey: true, IsNullable: false),
            new ColumnDefinition("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
            new ColumnDefinition("email", ColumnDataType.Varchar, Length: 200, IsUnique: true, IsNullable: false),
            new ColumnDefinition("phone", ColumnDataType.Varchar, Length: 20),
        ]), ct);

        var nameColumn = table.Columns.Single(c => c.Name == "name");
        var emailColumn = table.Columns.Single(c => c.Name == "email");
        var phoneColumn = table.Columns.Single(c => c.Name == "phone");

        var nameComponent = await components.CreateAsync(app.Id, new CreateComponentRequest(
            screen.Id, ComponentType.Input, "CustomerForm.name", 40, 40,
            PropertiesJson: """{"label":"Name"}""", ValidationJson: """{"required":true}""", DataBinding: "name"), ct);

        var emailComponent = await components.CreateAsync(app.Id, new CreateComponentRequest(
            screen.Id, ComponentType.Email, "CustomerForm.email", 40, 140,
            PropertiesJson: """{"label":"Email"}""", ValidationJson: """{"required":true}""", DataBinding: "email"), ct);

        var phoneComponent = await components.CreateAsync(app.Id, new CreateComponentRequest(
            screen.Id, ComponentType.Input, "CustomerForm.phone", 40, 240,
            PropertiesJson: """{"label":"Phone"}""", DataBinding: "phone"), ct);

        await components.CreateAsync(app.Id, new CreateComponentRequest(
            screen.Id, ComponentType.Button, "CustomerForm.save", 40, 340, PropertiesJson: """{"label":"Save"}"""), ct);

        var service = await serviceDefinitions.CreateAsync(app.Id,
            new CreateServiceRequest("CustomerService", table.Id, "Creates customer records."), ct);

        var api = await apis.CreateAsync(app.Id, new CreateApiEndpointRequest(
            ApiHttpMethod.Post, "/customer",
            RequestSchemaJson: """{"name":"string","email":"string","phone":"string"}""",
            ResponseSchemaJson: """{"id":"uuid","name":"string","email":"string","phone":"string"}""",
            ServiceId: service.Id, TableId: table.Id), ct);

        await mappings.CreateAsync(app.Id, new CreateMappingRequest(
            nameComponent.Id, "name", api.Id, "name", service.Id, "name", nameColumn.Id, TransformationType.Trim, null), ct);
        await mappings.CreateAsync(app.Id, new CreateMappingRequest(
            emailComponent.Id, "email", api.Id, "email", service.Id, "email", emailColumn.Id, TransformationType.Lowercase, null), ct);
        await mappings.CreateAsync(app.Id, new CreateMappingRequest(
            phoneComponent.Id, "phone", api.Id, "phone", service.Id, "phone", phoneColumn.Id, TransformationType.Trim, null), ct);

        await applications.PublishAsync(app.Id, ct);
    }

    private static async Task SeedDemoUserAsync(PlatformDbContext db, IServiceProvider services, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == DemoUserEmail, ct))
            return;

        var hasher = services.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = DemoUserEmail,
            PasswordHash = hasher.Hash(DemoUserPassword),
            DisplayName = "Demo Admin",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }
}
