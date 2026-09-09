using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserContext _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Settings> Settings => Set<Settings>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<AddOn>           AddOns          => Set<AddOn>();
    public DbSet<ItemAddOn>       ItemAddOns      => Set<ItemAddOn>();
    public DbSet<FoodItemRating>  FoodItemRatings => Set<FoodItemRating>();
    public DbSet<InventoryItem>      InventoryItems     => Set<InventoryItem>();
    public DbSet<StockTransaction>   StockTransactions  => Set<StockTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<LedgerEntry>          LedgerEntries          => Set<LedgerEntry>();
    public DbSet<Customer>             Customers              => Set<Customer>();
    public DbSet<PointsTransaction>    PointsTransactions     => Set<PointsTransaction>();
    public DbSet<SavedAddress>         SavedAddresses         => Set<SavedAddress>();
    public DbSet<AttendanceRecord>     AttendanceRecords      => Set<AttendanceRecord>();
    public DbSet<EmployeeSalary>       EmployeeSalaries       => Set<EmployeeSalary>();
    public DbSet<SalaryPayment>        SalaryPayments         => Set<SalaryPayment>();
    public DbSet<ShiftDefinition>      ShiftDefinitions       => Set<ShiftDefinition>();
    public DbSet<ShiftAssignment>      ShiftAssignments       => Set<ShiftAssignment>();
    public DbSet<PerformanceReview>    PerformanceReviews     => Set<PerformanceReview>();
    public DbSet<TableReservation>     TableReservations      => Set<TableReservation>();
    public DbSet<RolePermission>       RolePermissions        => Set<RolePermission>();
    public DbSet<Organization>              Organizations              => Set<Organization>();
    public DbSet<Driver>                    Drivers                    => Set<Driver>();
    public DbSet<DeliveryOrder>             DeliveryOrders             => Set<DeliveryOrder>();
    public DbSet<DeliveryChargeSlab>        DeliveryChargeSlabs        => Set<DeliveryChargeSlab>();
    public DbSet<ThirdPartyDeliveryConfig>  ThirdPartyDeliveryConfigs  => Set<ThirdPartyDeliveryConfig>();
    public DbSet<Promotion>                 Promotions                 => Set<Promotion>();
    public DbSet<License>                   Licenses                   => Set<License>();
    public DbSet<PlatformModule>            PlatformModules            => Set<PlatformModule>();
    public DbSet<OrganizationModule>        OrganizationModules        => Set<OrganizationModule>();
    public DbSet<Medicine>                  Medicines                  => Set<Medicine>();
    public DbSet<MedicineBatch>             MedicineBatches            => Set<MedicineBatch>();
    public DbSet<Prescription>              Prescriptions              => Set<Prescription>();
    public DbSet<PrescriptionItem>          PrescriptionItems          => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discovers and applies all IEntityTypeConfiguration<T>
        // classes defined in this assembly — no manual registration needed.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Tenant isolation ────────────────────────────────────────────────
        // _currentUser.OrganizationId == null means SuperAdmin (or, for anonymous
        // public endpoints, "no claims at all" — those endpoints must filter
        // explicitly with .IgnoreQueryFilters() + an explicit branchId instead of
        // relying on this ambient filter, since otherwise they'd see everything).
        modelBuilder.Entity<Category>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<FoodItem>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<Table>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<Order>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<Customer>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<InventoryItem>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<StockTransaction>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null ||
            (e.Branch.OrganizationId == _currentUser.OrganizationId &&
             (_currentUser.BranchId == null || e.BranchId == _currentUser.BranchId)));

        modelBuilder.Entity<Supplier>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<AddOn>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<SupplierPayment>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<Settings>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);

        modelBuilder.Entity<Promotion>().HasQueryFilter(e =>
            _currentUser.OrganizationId == null || e.OrganizationId == _currentUser.OrganizationId);
    }
}
