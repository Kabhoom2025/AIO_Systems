using Microsoft.EntityFrameworkCore;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data;

public class NovaErpDbContext : DbContext
{
    public NovaErpDbContext(DbContextOptions<NovaErpDbContext> options) : base(options) { }

    // Organization structure
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Branch>       Branches      => Set<Branch>();
    public DbSet<Department>   Departments   => Set<Department>();

    // Security / RBAC
    public DbSet<Role>               Roles               => Set<Role>();
    public DbSet<Permission>         Permissions         => Set<Permission>();
    public DbSet<RolePermission>     RolePermissions     => Set<RolePermission>();
    public DbSet<User>               Users               => Set<User>();
    public DbSet<RefreshToken>       RefreshTokens       => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LoginHistory>       LoginHistories      => Set<LoginHistory>();

    // Platform
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog>     AuditLogs     => Set<AuditLog>();

    // Settings / feature toggles
    public DbSet<OrganizationSettings> OrganizationSettings => Set<OrganizationSettings>();
    public DbSet<FeatureToggle>        FeatureToggles       => Set<FeatureToggle>();

    // Multi-currency
    public DbSet<Currency>     Currencies    => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    // Multi-language / localization
    public DbSet<Language>             Languages             => Set<Language>();
    public DbSet<OrganizationLanguage> OrganizationLanguages => Set<OrganizationLanguage>();

    // Document management
    public DbSet<Document> Documents => Set<Document>();

    // Approval workflow engine + workflow automation
    public DbSet<WorkflowDefinition>    WorkflowDefinitions    => Set<WorkflowDefinition>();
    public DbSet<WorkflowStepDefinition> WorkflowStepDefinitions => Set<WorkflowStepDefinition>();
    public DbSet<WorkflowInstance>      WorkflowInstances      => Set<WorkflowInstance>();
    public DbSet<WorkflowStepInstance>  WorkflowStepInstances  => Set<WorkflowStepInstance>();
    public DbSet<AutomationRule>        AutomationRules        => Set<AutomationRule>();

    // Scheduler
    public DbSet<ScheduledJobDefinition> ScheduledJobDefinitions => Set<ScheduledJobDefinition>();

    // Multi-channel notifications
    public DbSet<NotificationChannelSettings> NotificationChannelSettings => Set<NotificationChannelSettings>();
    public DbSet<NotificationDeliveryLog>     NotificationDeliveryLogs    => Set<NotificationDeliveryLog>();

    // Tax engine
    public DbSet<TaxCode>      TaxCodes      => Set<TaxCode>();
    public DbSet<TaxComponent> TaxComponents => Set<TaxComponent>();

    // CRM
    public DbSet<Lead>        Leads         => Set<Lead>();
    public DbSet<Account>     Accounts      => Set<Account>();
    public DbSet<Contact>     Contacts      => Set<Contact>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();

    // Sales
    public DbSet<SalesOrder>     SalesOrders     => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    // Procurement
    public DbSet<Vendor>          Vendors         => Set<Vendor>();
    public DbSet<RfqRequest>      RfqRequests     => Set<RfqRequest>();
    public DbSet<RfqItem>         RfqItems        => Set<RfqItem>();
    public DbSet<RfqVendorQuote>  RfqVendorQuotes => Set<RfqVendorQuote>();

    // Purchase
    public DbSet<PurchaseOrder>     PurchaseOrders     => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    // Inventory
    public DbSet<Product>       Products       => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Warehouse
    public DbSet<Warehouse>     Warehouses     => Set<Warehouse>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<Shipment>        Shipments        => Set<Shipment>();
    public DbSet<ShipmentLine>    ShipmentLines    => Set<ShipmentLine>();
    public DbSet<ShipmentPackage> ShipmentPackages => Set<ShipmentPackage>();
    public DbSet<ShipmentPackageItem> ShipmentPackageItems => Set<ShipmentPackageItem>();
    public DbSet<ShippingConnector> ShippingConnectors => Set<ShippingConnector>();
    public DbSet<ShippingConnectorFieldMapping> ShippingConnectorFieldMappings => Set<ShippingConnectorFieldMapping>();

    // Logistics
    public DbSet<Vehicle>      Vehicles      => Set<Vehicle>();
    public DbSet<DeliveryLoad> DeliveryLoads => Set<DeliveryLoad>();

    // Manufacturing
    public DbSet<BillOfMaterial>  BillOfMaterials  => Set<BillOfMaterial>();
    public DbSet<BomComponent>    BomComponents    => Set<BomComponent>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    // General Ledger
    public DbSet<LedgerAccount>   LedgerAccounts   => Set<LedgerAccount>();
    public DbSet<JournalEntry>    JournalEntries   => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    // Accounts Payable
    public DbSet<VendorBill>     VendorBills     => Set<VendorBill>();
    public DbSet<VendorBillLine> VendorBillLines => Set<VendorBillLine>();

    // Accounts Receivable
    public DbSet<CustomerInvoice>     CustomerInvoices     => Set<CustomerInvoice>();
    public DbSet<CustomerInvoiceLine> CustomerInvoiceLines => Set<CustomerInvoiceLine>();

    // Bank Reconciliation
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<BankStatementLine>  BankStatementLines  => Set<BankStatementLine>();

    // HRMS
    public DbSet<Employee>    Employees    => Set<Employee>();
    public DbSet<LeaveType>   LeaveTypes   => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // Payroll
    public DbSet<EmployeeCompensation> EmployeeCompensations => Set<EmployeeCompensation>();
    public DbSet<PayRun>     PayRuns     => Set<PayRun>();
    public DbSet<PayRunLine> PayRunLines => Set<PayRunLine>();

    // Projects
    public DbSet<Project>     Projects     => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    // Assets
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset>         Assets          => Set<Asset>();

    // Service Desk
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<ServiceTicket>  ServiceTickets   => Set<ServiceTicket>();

    // POS
    public DbSet<PosSale> PosSales => Set<PosSale>();

    // Retail
    public DbSet<Store> Stores => Set<Store>();

    // Dashboards
    public DbSet<Dashboard> Dashboards => Set<Dashboard>();

    // AI Assistant
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<AiAssistantSettings> AiAssistantSettings => Set<AiAssistantSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NovaErpDbContext).Assembly);
    }
}
