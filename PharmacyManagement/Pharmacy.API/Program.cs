using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.API.BackgroundServices;
using Pharmacy.API.Filters;
using Pharmacy.API.Hubs;
using Pharmacy.Application.Common;
using Pharmacy.Application.Interfaces;
using Pharmacy.Application.Services;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Authentication;
using Pharmacy.Infrastructure.Data;
using Pharmacy.Infrastructure.Repositories;
using Pharmacy.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PharmacyDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories + Services
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
builder.Services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOtpChallengeRepository, OtpChallengeRepository>();
builder.Services.AddScoped<IPatientAuthService, PatientAuthService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAiInsightsService, AiInsightsService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationSweepService>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// JWT — same secret as Restaurant API so a single login token works everywhere
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
    options.AddPolicy("patient", p => p.RequireClaim("tokenType", "patient"));
});
builder.Services.AddControllers(options => options.Filters.Add<AuditLogActionFilter>());
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PharmacyCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("PharmacyCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

// Auto-create schema + seed dev/demo data on startup in dev
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
    db.Database.EnsureCreated();
    await SeedDevDataAsync(db);
}

app.Run();

static async Task SeedDevDataAsync(PharmacyDbContext db)
{
    if (await db.Organizations.AnyAsync()) return; // already seeded

    // Permission catalog (system-wide, not org-scoped)
    var permissions = PermissionCatalog.All
        .Select(p => new Permission { Key = p.Key, Module = p.Module, Description = p.Description })
        .ToList();
    db.Permissions.AddRange(permissions);
    await db.SaveChangesAsync();

    Permission[] ByKeys(params string[] keys) =>
        permissions.Where(p => keys.Contains(p.Key)).ToArray();
    Permission[] ByModule(string module) =>
        permissions.Where(p => p.Module == module).ToArray();

    var allPermissions = permissions.ToArray();
    var superAdminRole = new Role { Name = "Super Admin", IsSystemRole = true, RolePermissions = allPermissions.Select(p => new RolePermission { Permission = p }).ToList() };
    var pharmacyAdminRole = new Role { Name = "Pharmacy Admin", IsSystemRole = true, RolePermissions = allPermissions.Select(p => new RolePermission { Permission = p }).ToList() };
    var pharmacistRole = new Role { Name = "Pharmacist", IsSystemRole = true, RolePermissions = ByModule("Medicines").Concat(ByModule("Prescriptions")).Concat(ByKeys("stock-adjustments.view", "stock-adjustments.create", "goods-receipts.view", "doctors.view", "patients.view", "patients.edit", "appointments.view")).Select(p => new RolePermission { Permission = p }).ToList() };
    var cashierRole = new Role { Name = "Cashier", IsSystemRole = true, RolePermissions = ByKeys("medicines.view", "prescriptions.view", "customers.view", "customers.create", "sales.view", "sales.create", "sales.edit", "deliveries.create").Select(p => new RolePermission { Permission = p }).ToList() };
    var inventoryManagerRole = new Role { Name = "Inventory Manager", IsSystemRole = true, RolePermissions = ByModule("Medicines").Concat(ByModule("StockAdjustments")).Concat(ByModule("GoodsReceipts")).Concat(ByKeys("purchase-orders.view")).Select(p => new RolePermission { Permission = p }).ToList() };
    var purchaseManagerRole = new Role { Name = "Purchase Manager", IsSystemRole = true, RolePermissions = ByModule("Suppliers").Concat(ByModule("PurchaseOrders")).Concat(ByModule("GoodsReceipts")).Concat(ByKeys("medicines.view")).Select(p => new RolePermission { Permission = p }).ToList() };
    var deliveryStaffRole = new Role { Name = "Delivery Staff", IsSystemRole = true, RolePermissions = ByKeys("prescriptions.view", "deliveries.view", "deliveries.edit").Select(p => new RolePermission { Permission = p }).ToList() };
    var accountantRole = new Role { Name = "Accountant", IsSystemRole = true, RolePermissions = ByModule("Expenses").Concat(ByKeys("reports.view", "purchase-orders.view", "suppliers.view")).Select(p => new RolePermission { Permission = p }).ToList() };
    var customerRole = new Role { Name = "Customer", IsSystemRole = true, RolePermissions = new List<RolePermission>() };

    db.Roles.AddRange(superAdminRole, pharmacyAdminRole, pharmacistRole, cashierRole,
        inventoryManagerRole, purchaseManagerRole, deliveryStaffRole, accountantRole, customerRole);

    var org = new Organization
    {
        Name = "Demo Pharmacy", Address = "12 MG Road, Bengaluru",
        Phone = "+91-9876543210", Email = "info@demopharmacy.local",
        LicenseNo = "DL-KA-2024-001", IsActive = true
    };

    var mainBranch = new Branch
    {
        Organization = org, Name = "Main Branch", Code = "MAIN", Type = "Retail",
        Address = "12 MG Road, Bengaluru", Phone = "+91-9876543210", IsActive = true
    };

    var adminUser = new User
    {
        Organization = org, Branch = mainBranch, Name = "Admin User", Email = "admin@pharmacy.local",
        PasswordHash = PasswordHasher.Hash("Admin@123"), Role = pharmacyAdminRole, IsActive = true
    };

    var deliveryUser = new User
    {
        Organization = org, Branch = mainBranch, Name = "Deepak Kumar", Email = "delivery@pharmacy.local",
        PasswordHash = PasswordHasher.Hash("Delivery@123"), Role = deliveryStaffRole, IsActive = true
    };

    var paracetamol = new Medicine
    {
        Organization = org, Name = "Paracetamol 500mg", GenericName = "Paracetamol",
        Category = "Tablets", DrugSchedule = "OTC", PackType = "Strip", PackSize = "10",
        Unit = "Tab", Manufacturer = "Cipla", HsnCode = "30049099", Sku = "SKU-000001", Barcode = "000000000001",
        MRP = 30, PurchasePrice = 18, GstPercent = 12, RackLocation = "A1", ReorderLevel = 50, IsActive = true
    };
    paracetamol.Batches.Add(new MedicineBatch
    {
        Branch = mainBranch, BatchNumber = "PCM-2401", ExpiryDate = DateTime.UtcNow.AddDays(20),
        ManufacturingDate = DateTime.UtcNow.AddMonths(-6), QuantityReceived = 200,
        CurrentQuantity = 180, PurchasePrice = 18
    });

    var amoxicillin = new Medicine
    {
        Organization = org, Name = "Amoxicillin 250mg", GenericName = "Amoxicillin",
        Category = "Capsules", DrugSchedule = "H", PackType = "Strip", PackSize = "10",
        Unit = "Cap", Manufacturer = "Sun Pharma", HsnCode = "30041000", Sku = "SKU-000002", Barcode = "000000000002",
        MRP = 80, PurchasePrice = 55, GstPercent = 12, RackLocation = "B2", ReorderLevel = 30, IsActive = true
    };
    amoxicillin.Batches.Add(new MedicineBatch
    {
        Branch = mainBranch, BatchNumber = "AMX-2402", ExpiryDate = DateTime.UtcNow.AddDays(75),
        ManufacturingDate = DateTime.UtcNow.AddMonths(-3), QuantityReceived = 100,
        CurrentQuantity = 90, PurchasePrice = 55
    });

    var coughSyrup = new Medicine
    {
        Organization = org, Name = "Cough Syrup 100ml", GenericName = "Dextromethorphan",
        Category = "Syrup", DrugSchedule = "OTC", PackType = "Bottle", PackSize = "100ml",
        Unit = "ML", Manufacturer = "Dabur", HsnCode = "30049011", Sku = "SKU-000003", Barcode = "000000000003",
        MRP = 120, PurchasePrice = 85, GstPercent = 12, RackLocation = "C1", ReorderLevel = 20, IsActive = true
    };

    var insulin = new Medicine
    {
        Organization = org, Name = "Insulin Injection", GenericName = "Human Insulin",
        Category = "Injection", DrugSchedule = "H1", PackType = "Vial", PackSize = "10ml",
        Unit = "ML", Manufacturer = "Novo Nordisk", HsnCode = "30043100", Sku = "SKU-000004", Barcode = "000000000004",
        MRP = 450, PurchasePrice = 380, GstPercent = 5, RackLocation = "Cold-1", ReorderLevel = 10, IsActive = true
    };

    var supplier1 = new Supplier
    {
        Organization = org, Name = "MedPlus Distributors", ContactPerson = "Ramesh Kumar",
        Phone = "+91-9988776655", Email = "ramesh@medplusdist.com", Address = "45 Industrial Area, Bengaluru",
        GstNumber = "29ABCDE1234F1Z5", PaymentTerms = "Net 30", CreditLimit = 500000, IsActive = true
    };
    var supplier2 = new Supplier
    {
        Organization = org, Name = "HealthCare Supplies Ltd", ContactPerson = "Priya Singh",
        Phone = "+91-9911223344", Email = "priya@healthcaresupplies.com", Address = "78 Commerce Street, Mumbai",
        GstNumber = "27XYZAB5678C1Z9", PaymentTerms = "Net 45", CreditLimit = 750000, IsActive = true
    };

    var doctor1 = new Doctor
    {
        Organization = org, Name = "Dr. Anil Sharma", RegistrationNumber = "KMC-45231",
        Specialty = "General Physician", Phone = "+91-9845012345", Email = "anil.sharma@example.com",
        HospitalName = "City Care Hospital", IsActive = true
    };
    var doctor2 = new Doctor
    {
        Organization = org, Name = "Dr. Kavitha Reddy", RegistrationNumber = "KMC-78120",
        Specialty = "Pediatrician", Phone = "+91-9845067890", Email = "kavitha.reddy@example.com",
        HospitalName = "Rainbow Children's Hospital", IsActive = true
    };

    var patient1 = new Patient
    {
        Organization = org, Name = "Ramesh Gowda", Phone = "+91-9900112233", Email = "ramesh.gowda@example.com",
        DateOfBirth = new DateTime(1985, 4, 12, 0, 0, 0, DateTimeKind.Utc), Gender = "Male", Address = "23 Lake View Road, Bengaluru",
        BloodGroup = "B+", Allergies = "Penicillin", ChronicDiseases = "Type 2 Diabetes",
        EmergencyContactName = "Suma Gowda", EmergencyContactPhone = "+91-9900112244",
        InsuranceProvider = "Star Health", InsurancePolicyNumber = "SH-2024-88213", IsActive = true
    };
    var patient2 = new Patient
    {
        Organization = org, Name = "Fatima Khan", Phone = "+91-9900556677", Email = "fatima.khan@example.com",
        DateOfBirth = new DateTime(1992, 11, 3, 0, 0, 0, DateTimeKind.Utc), Gender = "Female", Address = "56 Park Street, Bengaluru",
        BloodGroup = "O+", Allergies = "None known", ChronicDiseases = "Asthma",
        EmergencyContactName = "Imran Khan", EmergencyContactPhone = "+91-9900556688", IsActive = true
    };

    var customer1 = new Customer
    {
        Organization = org, Name = "Ramesh Gowda", Phone = "+91-9900112233", Email = "ramesh.gowda@example.com",
        Address = "23 Lake View Road, Bengaluru", MembershipLevel = "Gold", LoyaltyPoints = 420,
        WalletBalance = 150.50m, IsActive = true
    };
    var customer2 = new Customer
    {
        Organization = org, Name = "Fatima Khan", Phone = "+91-9900556677", Email = "fatima.khan@example.com",
        Address = "56 Park Street, Bengaluru", MembershipLevel = "Bronze", LoyaltyPoints = 35,
        WalletBalance = 0, IsActive = true
    };

    var expense1 = new Expense
    {
        Organization = org, Branch = mainBranch, Category = "Rent", Amount = 25000,
        ExpenseDate = DateTime.UtcNow.AddDays(-10), PaymentMethod = "Bank Transfer", Notes = "Monthly shop rent"
    };
    var expense2 = new Expense
    {
        Organization = org, Branch = mainBranch, Category = "Utilities", Amount = 3200,
        ExpenseDate = DateTime.UtcNow.AddDays(-4), PaymentMethod = "Cash", Notes = "Electricity bill"
    };

    var poItemSyrup = new PurchaseOrderItem { Medicine = coughSyrup, Quantity = 100, UnitPrice = 85 };
    var poItemInsulin = new PurchaseOrderItem { Medicine = insulin, Quantity = 20, UnitPrice = 380 };
    var purchaseOrder = new PurchaseOrder
    {
        Organization = org, Supplier = supplier1, PoNumber = "PO-DEMO-0001",
        OrderDate = DateTime.UtcNow.AddDays(-5), ExpectedDeliveryDate = DateTime.UtcNow.AddDays(2),
        Status = "Sent", Notes = "Demo seed purchase order"
    };
    purchaseOrder.Items.Add(poItemSyrup);
    purchaseOrder.Items.Add(poItemInsulin);
    purchaseOrder.TotalAmount = purchaseOrder.Items.Sum(i => i.Quantity * i.UnitPrice);

    db.Organizations.Add(org);
    db.Branches.Add(mainBranch);
    db.Users.AddRange(adminUser, deliveryUser);
    db.Medicines.AddRange(paracetamol, amoxicillin, coughSyrup, insulin);
    db.Suppliers.AddRange(supplier1, supplier2);
    db.Doctors.AddRange(doctor1, doctor2);
    db.Patients.AddRange(patient1, patient2);
    db.Customers.AddRange(customer1, customer2);
    db.Expenses.AddRange(expense1, expense2);
    db.PurchaseOrders.Add(purchaseOrder);
    await db.SaveChangesAsync();

    // Second pass: receive the PO (needs generated PurchaseOrderItem ids from the save above)
    var syrupBatch = new MedicineBatch
    {
        MedicineId = coughSyrup.Id, BranchId = mainBranch.Id, BatchNumber = "CS-2403", ExpiryDate = DateTime.UtcNow.AddMonths(18),
        ManufacturingDate = DateTime.UtcNow.AddDays(-10), QuantityReceived = 100, CurrentQuantity = 100, PurchasePrice = 85
    };
    var insulinBatch = new MedicineBatch
    {
        MedicineId = insulin.Id, BranchId = mainBranch.Id, BatchNumber = "INS-2404", ExpiryDate = DateTime.UtcNow.AddMonths(12),
        ManufacturingDate = DateTime.UtcNow.AddDays(-15), QuantityReceived = 10, CurrentQuantity = 10, PurchasePrice = 380
    };
    db.MedicineBatches.AddRange(syrupBatch, insulinBatch);

    var goodsReceipt = new GoodsReceipt
    {
        OrganizationId = org.Id, PurchaseOrder = purchaseOrder, ReceiptNumber = "GR-DEMO-0001",
        ReceivedDate = DateTime.UtcNow.AddDays(-1), ReceivedBy = "Admin User"
    };
    goodsReceipt.Items.Add(new GoodsReceiptItem
    {
        PurchaseOrderItemId = poItemSyrup.Id, MedicineId = coughSyrup.Id, BatchNumber = "CS-2403",
        ExpiryDate = syrupBatch.ExpiryDate, ManufacturingDate = syrupBatch.ManufacturingDate,
        QuantityReceived = 100, PurchasePrice = 85
    });
    goodsReceipt.Items.Add(new GoodsReceiptItem
    {
        PurchaseOrderItemId = poItemInsulin.Id, MedicineId = insulin.Id, BatchNumber = "INS-2404",
        ExpiryDate = insulinBatch.ExpiryDate, ManufacturingDate = insulinBatch.ManufacturingDate,
        QuantityReceived = 10, PurchasePrice = 380
    });
    poItemSyrup.ReceivedQuantity = 100;
    poItemInsulin.ReceivedQuantity = 10;
    purchaseOrder.Status = "PartiallyReceived";
    db.GoodsReceipts.Add(goodsReceipt);

    var paracetamolBatch = paracetamol.Batches.First();
    paracetamolBatch.CurrentQuantity -= 5;
    db.StockAdjustments.Add(new StockAdjustment
    {
        OrganizationId = org.Id, MedicineId = paracetamol.Id, MedicineBatch = paracetamolBatch,
        AdjustmentType = "Decrease", Quantity = 5, Reason = "Damaged",
        Notes = "Demo seed adjustment — damaged in transit", AdjustedDate = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
}
