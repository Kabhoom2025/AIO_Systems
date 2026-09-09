using System.Reflection;
using System.Text;
using FluentValidation;
using FoodOrder.API.Services;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Application.Services;
using FoodOrder.Application.Mappings;
using FoodOrder.Application.Validators.Auth;
using FoodOrder.Infrastructure.Authentication;
using FoodOrder.Infrastructure.Data;
using FoodOrder.Infrastructure.Repositories;
using FoodOrder.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FoodOrder.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddSwaggerWithJwt();
        services.AddCorsPolicy();
        services.AddHealthChecks();
        services.AddSignalR();
        services.AddRepositories();
        services.AddServices();
        services.AddValidation();
        services.AddMappings();

        return services;
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null))
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()!;
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            // Return JSON 401/403 instead of HTML challenge redirects.
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Unauthorized. Please login to access this resource."
                    });
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Forbidden. You do not have permission to access this resource."
                    });
                }
            };
        });

        services.AddAuthorization();
    }

    private static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Food Order Management API",
                Version = "v1",
                Description = "Restaurant Food Ordering System — Backend API"
            });

            // Wire up the XML documentation file generated from /// comments.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            // JWT bearer button in the Swagger UI.
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer {your JWT token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("FoodOrderCorsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IFoodItemRepository, FoodItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IAddOnRepository, AddOnRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITableReservationRepository, TableReservationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IPlatformModuleRepository, PlatformModuleRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<FoodOrder.Application.Interfaces.ICurrentUserContext, FoodOrder.API.Services.CurrentUserContext>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IFoodItemService, FoodItemService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISmsService, Msg91SmsService>();
        services.AddScoped<IWhatsAppService, UltraMsgWhatsAppService>();
        services.AddScoped<FoodOrder.Application.Interfaces.Services.IAddOnService, AddOnService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ILedgerNotifier, LedgerHubNotifier>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ITableReservationService, TableReservationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPlatformModuleService, PlatformModuleService>();
        services.AddHttpClient("msg91");
        services.AddHttpClient("ultramsg");
        services.AddHttpClient("sso");
        services.AddHttpClient("razorpay");
        services.AddScoped<IPaymentService, RazorpayPaymentService>();
        services.AddScoped<SsoService>();
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
    }

    private static void AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
    }
}
