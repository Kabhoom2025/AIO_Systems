using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data.Configurations;

namespace Pharmacy.Infrastructure.Data;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public DbSet<Organization>    Organizations    => Set<Organization>();
    public DbSet<User>            Users            => Set<User>();
    public DbSet<Medicine>        Medicines        => Set<Medicine>();
    public DbSet<MedicineBatch>   MedicineBatches  => Set<MedicineBatch>();
    public DbSet<Prescription>    Prescriptions    => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Supplier>          Suppliers          => Set<Supplier>();
    public DbSet<PurchaseOrder>     PurchaseOrders     => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt>      GoodsReceipts      => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem>  GoodsReceiptItems  => Set<GoodsReceiptItem>();
    public DbSet<StockAdjustment>   StockAdjustments   => Set<StockAdjustment>();
    public DbSet<Branch>            Branches           => Set<Branch>();
    public DbSet<Role>              Roles              => Set<Role>();
    public DbSet<Permission>        Permissions        => Set<Permission>();
    public DbSet<RolePermission>    RolePermissions    => Set<RolePermission>();
    public DbSet<Doctor>            Doctors            => Set<Doctor>();
    public DbSet<Patient>           Patients           => Set<Patient>();
    public DbSet<Customer>          Customers          => Set<Customer>();
    public DbSet<Sale>              Sales              => Set<Sale>();
    public DbSet<SaleItem>          SaleItems          => Set<SaleItem>();
    public DbSet<Expense>           Expenses           => Set<Expense>();
    public DbSet<Delivery>          Deliveries         => Set<Delivery>();
    public DbSet<Notification>      Notifications      => Set<Notification>();
    public DbSet<AuditLog>          AuditLogs          => Set<AuditLog>();
    public DbSet<OtpChallenge>      OtpChallenges      => Set<OtpChallenge>();
    public DbSet<Appointment>       Appointments       => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PharmacyDbContext).Assembly);
    }
}
