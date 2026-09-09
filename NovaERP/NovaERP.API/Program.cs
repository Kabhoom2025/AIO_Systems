using System.Text;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaERP.API.BackgroundServices;
using NovaERP.API.Filters;
using NovaERP.API.Hubs;
using NovaERP.API.Middleware;
using NovaERP.Application.Common;
using NovaERP.Application.Interfaces;
using NovaERP.Application.Mapping;
using NovaERP.Application.Services;
using NovaERP.Infrastructure.Authentication;
using NovaERP.Infrastructure.Data;
using NovaERP.Infrastructure.Jobs;
using NovaERP.Infrastructure.Repositories;
using NovaERP.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog — deliberate improvement over the HRMS template's default logging: structured
// console logging configured from appsettings so log level/format are ops-tunable per environment.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Database — provider switch is config-only (DatabaseProvider: "PostgreSql" | "SqlServer").
// The "Testing" environment (WebApplicationFactory in NovaERP.Tests) always gets an isolated
// EF Core InMemory database instead, so integration tests don't require a real server.
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "PostgreSql";
var testingDbName = Guid.NewGuid().ToString();
builder.Services.AddDbContext<NovaErpDbContext>(opts =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        opts.UseInMemoryDatabase(testingDbName);
    else if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
    else
        opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Organization structure
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// RBAC
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();

// Platform services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Settings + feature toggles
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IFeatureToggleRepository, FeatureToggleRepository>();
builder.Services.AddScoped<IFeatureToggleService, FeatureToggleService>();

// Multi-currency
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// Multi-language / localization
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IOrganizationLanguageRepository, OrganizationLanguageRepository>();
builder.Services.AddScoped<IOrganizationLanguageService, OrganizationLanguageService>();

// Document management
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Approval workflow engine
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
builder.Services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();

// Workflow automation
builder.Services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();
builder.Services.AddScoped<IAutomationRuleService, AutomationRuleService>();
builder.Services.AddScoped<IAutomationEngine, AutomationEngine>();

// Scheduler (Hangfire-backed recurring jobs)
builder.Services.AddScoped<IScheduledJobRepository, ScheduledJobRepository>();
builder.Services.AddScoped<IScheduledJobService, ScheduledJobService>();
builder.Services.AddScoped<ApprovalReminderJob>();
builder.Services.AddScoped<StaleExchangeRateCheckJob>();

// Multi-channel notifications (Email/SMS/Push)
builder.Services.AddScoped<INotificationChannelSettingsRepository, NotificationChannelSettingsRepository>();
builder.Services.AddScoped<INotificationChannelSettingsService, NotificationChannelSettingsService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ISmsSender, HttpSmsSender>();
builder.Services.AddScoped<IPushSender, HttpPushSender>();
builder.Services.AddScoped<IMultiChannelNotificationDispatcher, MultiChannelNotificationDispatcher>();
builder.Services.AddHttpClient();

// Tax engine
builder.Services.AddScoped<ITaxCodeRepository, TaxCodeRepository>();
builder.Services.AddScoped<ITaxCodeService, TaxCodeService>();

// CRM
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IOpportunityRepository, OpportunityRepository>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();

// Sales
builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();

// Procurement
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IRfqRequestRepository, RfqRequestRepository>();
builder.Services.AddScoped<IRfqRequestService, RfqRequestService>();

// Purchase
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

// Inventory
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();

// Warehouse
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// Shipping Rate Connectors — generic, mapping-driven TMS/rate-shop integration
builder.Services.AddScoped<IShippingConnectorRepository, ShippingConnectorRepository>();
builder.Services.AddScoped<IShippingConnectorService, ShippingConnectorService>();
builder.Services.AddScoped<IShippingConnectorRunner, ShippingConnectorHttpRunner>();
builder.Services.AddScoped<IShippingRateService, ShippingRateService>();
builder.Services.AddHttpClient(nameof(ShippingConnectorHttpRunner));

// Manufacturing
builder.Services.AddScoped<IBillOfMaterialRepository, BillOfMaterialRepository>();
builder.Services.AddScoped<IBillOfMaterialService, BillOfMaterialService>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

// General Ledger
builder.Services.AddScoped<ILedgerAccountRepository, LedgerAccountRepository>();
builder.Services.AddScoped<ILedgerAccountService, LedgerAccountService>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();

// Accounts Payable
builder.Services.AddScoped<IVendorBillRepository, VendorBillRepository>();
builder.Services.AddScoped<IVendorBillService, VendorBillService>();

// Accounts Receivable
builder.Services.AddScoped<ICustomerInvoiceRepository, CustomerInvoiceRepository>();
builder.Services.AddScoped<ICustomerInvoiceService, CustomerInvoiceService>();

// Bank Reconciliation
builder.Services.AddScoped<IBankReconciliationRepository, BankReconciliationRepository>();
builder.Services.AddScoped<IBankReconciliationService, BankReconciliationService>();

// HRMS
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

// Payroll
builder.Services.AddScoped<IEmployeeCompensationRepository, EmployeeCompensationRepository>();
builder.Services.AddScoped<IEmployeeCompensationService, EmployeeCompensationService>();
builder.Services.AddScoped<IPayRunRepository, PayRunRepository>();
builder.Services.AddScoped<IPayRunService, PayRunService>();

// Employee Portal
builder.Services.AddScoped<IEmployeePortalService, EmployeePortalService>();

// Projects
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();

// Assets
builder.Services.AddScoped<IAssetCategoryRepository, AssetCategoryRepository>();
builder.Services.AddScoped<IAssetCategoryService, AssetCategoryService>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetService, AssetService>();

// Logistics
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IDeliveryLoadRepository, DeliveryLoadRepository>();
builder.Services.AddScoped<IDeliveryLoadService, DeliveryLoadService>();

// Service Desk
builder.Services.AddScoped<ITicketCategoryRepository, TicketCategoryRepository>();
builder.Services.AddScoped<ITicketCategoryService, TicketCategoryService>();
builder.Services.AddScoped<IServiceTicketRepository, ServiceTicketRepository>();
builder.Services.AddScoped<IServiceTicketService, ServiceTicketService>();

// POS
builder.Services.AddScoped<IPosSaleRepository, PosSaleRepository>();
builder.Services.AddScoped<IPosSaleService, PosSaleService>();

// Retail
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();

// Customer Portal
builder.Services.AddScoped<ICustomerPortalService, CustomerPortalService>();

// Vendor Portal
builder.Services.AddScoped<IVendorPortalService, VendorPortalService>();

// Dashboard Builder
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IWidgetDataService, WidgetDataService>();

// Reports
builder.Services.AddScoped<IReportsService, ReportsService>();

// AI Assistant
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();
builder.Services.AddScoped<IAiAssistantSettingsRepository, AiAssistantSettingsRepository>();
builder.Services.AddScoped<IAiAssistantSettingsService, AiAssistantSettingsService>();
builder.Services.AddHttpClient<AnthropicAiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com");
});
builder.Services.AddHttpClient<GroqAiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com");
});
builder.Services.AddScoped<IAiProvider, AiProviderRouter>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationSweepService>();

// Ensure our own schema exists BEFORE Hangfire gets a chance to install its "hangfire" schema
// below. AddHangfire's storage configuration installs Hangfire's tables eagerly, synchronously,
// as soon as it runs — on a fresh database that happens before the app's own EnsureCreated() call
// further down. EnsureCreated's "does the database already have tables" check isn't scoped to a
// schema, so it would see Hangfire's tables and skip creating ours entirely. Building a short-lived
// context here (outside the DI container, which isn't built yet) sidesteps that ordering trap.
if (!builder.Environment.IsEnvironment("Testing"))
{
    var pregameOptions = new DbContextOptionsBuilder<NovaErpDbContext>();
    if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        pregameOptions.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
    else
        pregameOptions.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    using var pregameDb = new NovaErpDbContext(pregameOptions.Options);
    pregameDb.Database.EnsureCreated();
}

// Hangfire — storage provider switch mirrors the AddDbContext provider switch above exactly, so
// the same DatabaseProvider config value drives both. Hangfire creates its own "hangfire"
// schema/tables inside the same Postgres/SQL Server database as the main app; no separate
// database is provisioned. The "Testing" environment always gets isolated in-memory storage so
// WebApplicationFactory-based integration tests never need a real database or a running server.
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings();

    if (builder.Environment.IsEnvironment("Testing"))
        cfg.UseMemoryStorage();
    else if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        cfg.UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlServerConnection"));
    else
        // The string-connection-string overload is marked obsolete in favor of the options-lambda
        // form in 1.21.x previews, but is still fully supported and simplest here; suppressed
        // deliberately rather than chasing a moving pre-2.0 API surface.
#pragma warning disable CS0618
        cfg.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
#pragma warning restore CS0618
});
builder.Services.AddHangfireServer();

// FluentValidation — validators discovered from the Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(PermissionCatalog).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

// JWT — same secret as the other AIO services so a single login token works everywhere
var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
            ValidateIssuer   = true,
            ValidIssuer      = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience    = jwt["Audience"],
            ValidateLifetime = true
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/notifications"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionCatalog.All)
        options.AddPolicy(permission.Key, p => p.Requirements.Add(new PermissionRequirement(permission.Key)));
});

builder.Services.AddControllers(options => options.Filters.Add<AuditLogActionFilter>());
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NovaErpCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("NovaErpCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "NovaERP.API", timeUtc = DateTime.UtcNow }));

// Hangfire dashboard — Development only, guarded by the local-requests-only recipe rather than
// real multi-user auth. This is a deliberate, documented scope boundary (see
// LocalRequestsOnlyAuthorizationFilter): a real production deployment of this dashboard would
// need real auth wired to the JWT/permission system.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
    });
}

// Auto-create schema + seed demo data on startup in dev
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
    db.Database.EnsureCreated();
    await SeedData.SeedAsync(db);

    // Register each enabled ScheduledJobDefinition row as a Hangfire recurring job. JobKey maps
    // to the concrete job class via RecurringJobRegistrar; unknown keys are skipped rather than
    // crashing startup, since new job classes will be added over time.
    var jobRepo = scope.ServiceProvider.GetRequiredService<IScheduledJobRepository>();
    foreach (var job in await jobRepo.GetAllEnabledAsync())
    {
        try
        {
            RecurringJobRegistrar.Register(job.JobKey, job.Id, job.CronExpression, job.IsEnabled);
        }
        catch (InvalidOperationException)
        {
            // Unknown JobKey — leave unregistered rather than failing app startup.
        }
    }
}

app.Run();

/// <summary>Exposed so WebApplicationFactory&lt;Program&gt; can be used in integration tests.</summary>
public partial class Program { }
