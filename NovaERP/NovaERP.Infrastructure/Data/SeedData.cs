using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Authentication;

namespace NovaERP.Infrastructure.Data;

/// <summary>One-time demo data seeded on first run against an empty NovaERPDB (dev only).</summary>
public static class SeedData
{
    public static async Task SeedAsync(NovaErpDbContext db)
    {
        if (await db.Organizations.AnyAsync()) return; // already seeded

        // Permission catalog (system-wide, not org-scoped)
        var permissions = PermissionCatalog.All
            .Select(p => new Permission { Key = p.Key, Module = p.Module, Description = p.Description })
            .ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        Permission[] ByKeys(params string[] keys) => permissions.Where(p => keys.Contains(p.Key)).ToArray();
        var allPermissions = permissions.ToArray();

        // Foundation-module keys only (this phase has controllers for these).
        // NOTE: "documents" and "workflows" were added after this list was first written
        // (Phase 1's Document module and Phase 2's Approval Workflow Engine) — both are
        // included here so Manager/Employee actually get the permissions the seeded demo
        // data assumes they have (the seeded workflow's step 1 approver is the Manager
        // role, so Manager needs workflows.view/edit or it can never act on its own step).
        // "automation" and "scheduler" are deliberately left Admin-only: configuring
        // automation rules and cron jobs is an admin/ops concern, not something a line
        // manager or employee should have access to. "tax" is included here (Phase 3):
        // tax codes are read-heavy reference data other modules' line items will look
        // up, so Manager/Employee need at least view access like they do for documents.
        // "crm" (Phase 3) is included too — Leads/Accounts/Contacts/Opportunities are an
        // operational module reps and managers use daily, not an admin-only concern.
        // "sales" (Phase 3) follows the same reasoning — sales orders are day-to-day work.
        // "procurement" (Phase 3) follows too — vendor/RFQ management is operational.
        // "purchase" (Phase 3) closes out the phase — purchase orders are day-to-day work.
        // "inventory" (Phase 4) opens the next phase — stock levels are read-heavy reference
        // data warehouse staff and sales/purchase reps both look up daily.
        // "warehouse" follows the same reasoning — warehouse locations and stock transfers
        // are exactly the kind of read-heavy operational data warehouse staff need daily.
        // "manufacturing" closes out Phase 4 — bills of materials and production orders are
        // day-to-day work for production staff and managers, same as sales/purchase orders.
        // "finance" (Phase 5 — General Ledger) is deliberately NOT included here: chart of
        // accounts and journal entries are sensitive financial records, not day-to-day
        // operational data like inventory/sales — same "admin/ops concern" reasoning already
        // applied to "automation"/"scheduler".
        var foundationModules = new[]
        {
            "organization", "branches", "departments", "users", "roles",
            "notifications", "audit-logs", "settings", "documents", "workflows", "tax", "crm", "sales", "procurement", "purchase", "inventory", "warehouse", "manufacturing",
            "projects", "assets", "service-desk", "pos", "retail", "logistics", "carrier-connector"
        };
        var viewCreateEditKeys = permissions
            .Where(p => foundationModules.Contains(p.Key.Split('.')[0]) &&
                        (p.Key.EndsWith(".view") || p.Key.EndsWith(".create") || p.Key.EndsWith(".edit")))
            .Select(p => p.Key)
            .ToArray();
        var viewOnlyKeys = permissions
            .Where(p => foundationModules.Contains(p.Key.Split('.')[0]) && p.Key.EndsWith(".view"))
            .Select(p => p.Key)
            .ToArray();

        var adminRole = new Role
        {
            Name = "Admin", IsSystemRole = true,
            RolePermissions = allPermissions.Select(p => new RolePermission { Permission = p }).ToList()
        };
        var managerRole = new Role
        {
            Name = "Manager", IsSystemRole = true,
            RolePermissions = ByKeys(viewCreateEditKeys)
                .Select(p => new RolePermission { Permission = p }).ToList()
        };
        var employeeRole = new Role
        {
            Name = "Employee", IsSystemRole = true,
            RolePermissions = ByKeys(viewOnlyKeys)
                .Select(p => new RolePermission { Permission = p }).ToList()
        };

        db.Roles.AddRange(adminRole, managerRole, employeeRole);

        var org = new Organization
        {
            Name = "NovaERP Demo Org", Code = "NOVAERP",
            LegalName = "NovaERP Demo Org Pvt Ltd",
            Address = "Bagmane Tech Park, Bengaluru, Karnataka, India",
            Phone = "+91-9876500000", Email = "info@novaerp.local", Website = "https://novaerp.local",
            TaxNumber = "29AACCT1234F1Z9", Timezone = "Asia/Kolkata", Currency = "INR", IsActive = true
        };

        var headOffice = new Branch
        {
            Organization = org, Name = "Bengaluru Head Office", Code = "BLR-HO",
            Address = "Bagmane Tech Park, Bengaluru", City = "Bengaluru", State = "Karnataka", Country = "India",
            Phone = "+91-9876500000", Email = "blr@novaerp.local", Timezone = "Asia/Kolkata",
            IsHeadOffice = true, IsActive = true
        };
        var puneBranch = new Branch
        {
            Organization = org, Name = "Pune Office", Code = "PUN-01",
            Address = "Hinjewadi Phase 2, Pune", City = "Pune", State = "Maharashtra", Country = "India",
            Phone = "+91-9876500001", Email = "pune@novaerp.local", Timezone = "Asia/Kolkata",
            IsHeadOffice = false, IsActive = true
        };

        var engineeringDept = new Department { Organization = org, Branch = headOffice, Name = "Engineering", Code = "ENG", Description = "Product engineering", IsActive = true };
        var financeDept     = new Department { Organization = org, Branch = headOffice, Name = "Finance", Code = "FIN", Description = "Finance & accounts", IsActive = true };
        var salesDept       = new Department { Organization = org, Branch = puneBranch, Name = "Sales", Code = "SALES", Description = "Sales & business development", IsActive = true };

        db.Organizations.Add(org);
        db.Branches.AddRange(headOffice, puneBranch);
        db.Departments.AddRange(engineeringDept, financeDept, salesDept);
        await db.SaveChangesAsync();

        var adminUser = new User
        {
            Organization = org, Branch = headOffice, Name = "NovaERP Admin",
            Email = "admin@novaerp.local", PasswordHash = PasswordHasher.Hash("Admin@123"), Role = adminRole, IsActive = true
        };
        var managerUser = new User
        {
            Organization = org, Branch = headOffice, Name = "Demo Manager",
            Email = "manager@novaerp.local", PasswordHash = PasswordHasher.Hash("Manager@123"), Role = managerRole, IsActive = true
        };
        var employeeUser = new User
        {
            Organization = org, Branch = puneBranch, Name = "Demo Employee",
            Email = "employee@novaerp.local", PasswordHash = PasswordHasher.Hash("Employee@123"), Role = employeeRole, IsActive = true
        };
        // customerUser reuses employeeRole since the Customer Portal never checks permissions
        // (see CustomerPortalService's doc comment) — same "no new role needed" reasoning as
        // the Employee Portal module reusing employeeUser/priyaEmployee without a new role.
        var customerUser = new User
        {
            Organization = org, Branch = headOffice, Name = "Priya Nair (Globex)",
            Email = "customer@novaerp.local", PasswordHash = PasswordHasher.Hash("Customer@123"), Role = employeeRole, IsActive = true
        };
        // vendorUser reuses employeeRole for the same reason as customerUser — the Vendor
        // Portal never checks permissions either (see VendorPortalService's doc comment).
        var vendorUser = new User
        {
            Organization = org, Branch = headOffice, Name = "Bharat Steel Traders",
            Email = "vendor@novaerp.local", PasswordHash = PasswordHasher.Hash("Vendor@123"), Role = employeeRole, IsActive = true
        };
        db.Users.AddRange(adminUser, managerUser, employeeUser, customerUser, vendorUser);
        await db.SaveChangesAsync();

        db.Notifications.Add(new Notification
        {
            Organization = org, UserId = null, Title = "Welcome to NovaERP",
            Message = "Your NovaERP foundation module is up and running.", Type = "Info"
        });

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = org.Id, UserId = adminUser.Id, UserName = adminUser.Name,
            Action = "Seed Demo Data", Method = "SYSTEM", Path = "/seed", StatusCode = 200, DurationMs = 0
        });

        // Organization settings — one row, lean platform defaults.
        db.OrganizationSettings.Add(new OrganizationSettings
        {
            OrganizationId = org.Id,
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "INR",
            DefaultTimezone = "Asia/Kolkata",
            DateFormat = "dd/MM/yyyy",
            TimeFormat = "HH:mm",
            FiscalYearStartMonth = 4,
            InvoiceNumberPrefix = "INV",
            TaxInclusivePricing = false
        });

        // Feature toggles — one row per catalog module; foundation modules (this phase has
        // controllers for these, Documents included) default on, everything else "coming soon".
        var enabledModuleKeys = new[]
        {
            "organization", "branches", "departments", "users", "roles",
            "notifications", "audit-logs", "settings", "documents"
        };
        var moduleKeys = PermissionCatalog.All.Select(p => p.Key.Split('.')[0]).Distinct();
        db.FeatureToggles.AddRange(moduleKeys.Select(key => new FeatureToggle
        {
            Organization = org,
            ModuleKey = key,
            IsEnabled = enabledModuleKeys.Contains(key)
        }));

        // Currencies — system-wide reference data.
        var inr = new Currency { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2, IsActive = true };
        var usd = new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, IsActive = true };
        var eur = new Currency { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, IsActive = true };
        var gbp = new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, IsActive = true };
        var aed = new Currency { Code = "AED", Name = "UAE Dirham", Symbol = "د.إ", DecimalPlaces = 2, IsActive = true };
        var sgd = new Currency { Code = "SGD", Name = "Singapore Dollar", Symbol = "S$", DecimalPlaces = 2, IsActive = true };
        var aud = new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, IsActive = true };
        var cad = new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, IsActive = true };
        var jpy = new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, IsActive = true };
        var cny = new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, IsActive = true };
        db.Currencies.AddRange(inr, usd, eur, gbp, aed, sgd, aud, cad, jpy, cny);

        // Sample exchange rates for the demo org, INR as base.
        var effectiveDate = DateTime.UtcNow.Date;
        db.ExchangeRates.AddRange(
            new ExchangeRate { Organization = org, FromCurrencyCode = "INR", ToCurrencyCode = "USD", Rate = 0.012m, EffectiveDate = effectiveDate },
            new ExchangeRate { Organization = org, FromCurrencyCode = "INR", ToCurrencyCode = "EUR", Rate = 0.011m, EffectiveDate = effectiveDate },
            new ExchangeRate { Organization = org, FromCurrencyCode = "INR", ToCurrencyCode = "GBP", Rate = 0.0095m, EffectiveDate = effectiveDate },
            new ExchangeRate { Organization = org, FromCurrencyCode = "INR", ToCurrencyCode = "AED", Rate = 0.044m, EffectiveDate = effectiveDate }
        );

        // Sample tax codes for the demo org (Phase 3 — Tax Engine). GST18/GST5 are
        // compound (CGST + SGST components); ZERO is a single 0% component for exempt items.
        var gst18Code = new TaxCode
        {
            Organization = org, Code = "GST18", Name = "GST 18%", IsActive = true,
            Components = new List<TaxComponent>
            {
                new() { Name = "CGST", RatePercent = 9m, DisplayOrder = 1 },
                new() { Name = "SGST", RatePercent = 9m, DisplayOrder = 2 }
            }
        };
        var gst5Code = new TaxCode
        {
            Organization = org, Code = "GST5", Name = "GST 5%", IsActive = true,
            Components = new List<TaxComponent>
            {
                new() { Name = "CGST", RatePercent = 2.5m, DisplayOrder = 1 },
                new() { Name = "SGST", RatePercent = 2.5m, DisplayOrder = 2 }
            }
        };
        db.TaxCodes.AddRange(
            gst18Code,
            gst5Code,
            new TaxCode
            {
                Organization = org, Code = "ZERO", Name = "Exempt (0%)", IsActive = true,
                Components = new List<TaxComponent>
                {
                    new() { Name = "Exempt", RatePercent = 0m, DisplayOrder = 1 }
                }
            }
        );

        // Sample CRM data for the demo org (Phase 3 — CRM). Two Accounts each with a Contact,
        // a couple of Opportunities across different stages, and three Leads — one already
        // converted (with ConvertedAccountId set) to demonstrate the convert flow's result.
        var acmeAccount = new Account { Organization = org, Name = "Acme Manufacturing", Industry = "Manufacturing", Website = "https://acme.example", Phone = "+91-9876512345", Owner = managerUser };
        var globexAccount = new Account { Organization = org, Name = "Globex Retail", Industry = "Retail", Website = "https://globex.example", Phone = "+91-9876554321", Owner = managerUser };
        db.Accounts.AddRange(acmeAccount, globexAccount);

        db.Contacts.AddRange(
            new Contact { Organization = org, Account = acmeAccount, FirstName = "Rahul", LastName = "Verma", Email = "rahul.verma@acme.example", Phone = "+91-9876512346", Title = "Procurement Manager", Owner = managerUser },
            // Linked to customerUser (customer@novaerp.local) so the Customer Portal module
            // can be demoed live using her existing Globex SalesOrder/CustomerInvoice records.
            new Contact { Organization = org, Account = globexAccount, User = customerUser, FirstName = "Priya", LastName = "Nair", Email = "priya.nair@globex.example", Phone = "+91-9876554322", Title = "Store Operations Lead", Owner = employeeUser }
        );

        db.Opportunities.AddRange(
            new Opportunity { Organization = org, Account = acmeAccount, Name = "Acme ERP Rollout — Phase 1", Amount = 850000m, Stage = "Proposal", CloseDate = DateTime.UtcNow.Date.AddDays(30), Owner = managerUser },
            new Opportunity { Organization = org, Account = globexAccount, Name = "Globex POS Upgrade", Amount = 220000m, Stage = "Qualification", CloseDate = DateTime.UtcNow.Date.AddDays(45), Owner = employeeUser }
        );

        db.Leads.AddRange(
            new Lead { Organization = org, Name = "Suresh Kumar", CompanyName = "Kumar Textiles", Email = "suresh@kumartextiles.example", Phone = "+91-9876500002", Source = "Website", Status = "New", Owner = employeeUser },
            new Lead { Organization = org, Name = "Anita Desai", CompanyName = "Desai Logistics", Email = "anita@desailogistics.example", Phone = "+91-9876500003", Source = "Referral", Status = "Qualified", Owner = managerUser },
            new Lead { Organization = org, Name = "Rahul Verma", CompanyName = "Acme Manufacturing", Email = "rahul.verma@acme.example", Phone = "+91-9876512346", Source = "Trade Show", Status = "Converted", Owner = managerUser, ConvertedAccount = acmeAccount, ConvertedDate = DateTime.UtcNow.Date.AddDays(-14) }
        );

        // Sample products for the demo org (Phase 4 — Inventory). Sales/Purchase order lines
        // below link to these via the new ProductId retrofit, demonstrating it alongside the
        // existing free-text ItemName. Opening stock balances are seeded further below as
        // standalone Receipt movements rather than trying to backfill movements matching every
        // already-Confirmed/Received seeded order (those orders' Ids don't exist until the
        // final SaveChangesAsync, so a matching EntityId isn't available at this point).
        var steelSheetProduct = new Product { Organization = org, Sku = "STEEL-2MM", Name = "Cold-rolled steel sheet (2mm)", UnitOfMeasure = "EA", UnitCost = 850m, IsActive = true };
        var steelBeamProduct = new Product { Organization = org, Sku = "STEEL-BEAM", Name = "Structural steel beams", UnitOfMeasure = "EA", UnitCost = 12500m, IsActive = true };
        var posTerminalProduct = new Product { Organization = org, Sku = "POS-TERM", Name = "POS Terminal Hardware", UnitOfMeasure = "EA", UnitCost = 18000m, IsActive = true };
        var cartonProduct = new Product { Organization = org, Sku = "CARTON-EXP", Name = "Corrugated export cartons", UnitOfMeasure = "EA", UnitCost = 45m, IsActive = true };
        db.Products.AddRange(steelSheetProduct, steelBeamProduct, posTerminalProduct, cartonProduct);

        // Sample warehouses for the demo org (Phase 4 — Warehouse), each tied to one of the
        // already-seeded Branches.
        var bengaluruWarehouse = new Warehouse { Organization = org, Branch = headOffice, Name = "Bengaluru Main Warehouse", Code = "WH-BLR", Address = "Bagmane Tech Park, Bengaluru", IsActive = true };
        var puneWarehouse = new Warehouse { Organization = org, Branch = puneBranch, Name = "Pune Regional Warehouse", Code = "WH-PUN", Address = "Hinjewadi Phase 2, Pune", IsActive = true };
        db.Warehouses.AddRange(bengaluruWarehouse, puneWarehouse);

        // Sample sales orders for the demo org (Phase 3 — Sales). Seed data bypasses
        // SalesOrderService so OrderNumber is set explicitly here (the service generates it
        // from the row's own Id on create, which isn't available until after a real save).
        var globexPosTerminalLine = new SalesOrderLine { ItemName = "POS Terminal Hardware", Product = posTerminalProduct, Quantity = 10m, UnitPrice = 18000m, TaxCode = gst5Code, TaxRatePercent = 5m, DisplayOrder = 1 };
        var globexSalesOrder = new SalesOrder
        {
            Organization = org, OrderNumber = "SO-00002", Account = globexAccount, Status = "Confirmed",
            OrderDate = DateTime.UtcNow.Date.AddDays(-5), Owner = employeeUser,
            Lines = new List<SalesOrderLine> { globexPosTerminalLine }
        };
        db.SalesOrders.AddRange(
            new SalesOrder
            {
                Organization = org, OrderNumber = "SO-00001", Account = acmeAccount, Status = "Draft",
                OrderDate = DateTime.UtcNow.Date, Owner = managerUser,
                Lines = new List<SalesOrderLine>
                {
                    new() { ItemName = "ERP Implementation — Consulting Hours", Quantity = 100m, UnitPrice = 2500m, TaxCode = gst18Code, TaxRatePercent = 18m, DisplayOrder = 1 },
                    new() { ItemName = "On-site Training (3 days)", Quantity = 3m, UnitPrice = 15000m, TaxCode = gst18Code, TaxRatePercent = 18m, DisplayOrder = 2 }
                }
            },
            globexSalesOrder
        );

        // Sample vendors + RFQs for the demo org (Phase 3 — Procurement). Seed data bypasses
        // RfqRequestService so RfqNumber is set explicitly here (same reason as SalesOrder above).
        // Linked to vendorUser (vendor@novaerp.local) so the Vendor Portal module can be
        // demoed live using its existing PurchaseOrder/VendorBill records.
        var steelVendor = new Vendor { Organization = org, User = vendorUser, Name = "Bharat Steel Traders", Category = "Raw Materials", ContactEmail = "sales@bharatsteel.example", ContactPhone = "+91-9876511111", IsActive = true, Owner = managerUser };
        var packagingVendor = new Vendor { Organization = org, Name = "SafePack Industries", Category = "Packaging", ContactEmail = "orders@safepack.example", ContactPhone = "+91-9876522222", IsActive = true, Owner = employeeUser };
        db.Vendors.AddRange(steelVendor, packagingVendor);

        db.RfqRequests.AddRange(
            new RfqRequest
            {
                Organization = org, RfqNumber = "RFQ-00001", Title = "Steel sheets for Q3 production", Status = "Sent",
                IssueDate = DateTime.UtcNow.Date.AddDays(-3), ResponseDeadline = DateTime.UtcNow.Date.AddDays(7), Owner = managerUser,
                Items = new List<RfqItem>
                {
                    new() { ItemName = "Cold-rolled steel sheet (2mm)", Quantity = 500m, DisplayOrder = 1 }
                },
                Quotes = new List<RfqVendorQuote>
                {
                    new() { Vendor = steelVendor, QuotedAmount = 425000m, Notes = "Includes freight", RespondedDate = DateTime.UtcNow.Date.AddDays(-1) },
                    new() { Vendor = packagingVendor }
                }
            },
            new RfqRequest
            {
                Organization = org, RfqNumber = "RFQ-00002", Title = "Export packaging materials", Status = "Draft",
                IssueDate = DateTime.UtcNow.Date, Owner = employeeUser,
                Items = new List<RfqItem>
                {
                    new() { ItemName = "Corrugated export cartons", Quantity = 2000m, DisplayOrder = 1 },
                    new() { ItemName = "Pallet wrap rolls", Quantity = 100m, DisplayOrder = 2 }
                },
                Quotes = new List<RfqVendorQuote>
                {
                    new() { Vendor = packagingVendor }
                }
            }
        );

        // Sample purchase orders for the demo org (Phase 3 — Purchase). Seed data bypasses
        // PurchaseOrderService so PoNumber is set explicitly here (same reason as SalesOrder/RfqRequest above).
        db.PurchaseOrders.AddRange(
            new PurchaseOrder
            {
                Organization = org, PoNumber = "PO-00001", Vendor = steelVendor, Status = "Draft",
                OrderDate = DateTime.UtcNow.Date, Owner = managerUser,
                Lines = new List<PurchaseOrderLine>
                {
                    new() { ItemName = "Cold-rolled steel sheet (2mm)", Product = steelSheetProduct, Quantity = 500m, UnitPrice = 850m, TaxCode = gst18Code, TaxRatePercent = 18m, DisplayOrder = 1 }
                }
            },
            new PurchaseOrder
            {
                Organization = org, PoNumber = "PO-00002", Vendor = packagingVendor, Status = "Confirmed",
                OrderDate = DateTime.UtcNow.Date.AddDays(-4), Owner = employeeUser,
                Lines = new List<PurchaseOrderLine>
                {
                    new() { ItemName = "Corrugated export cartons", Quantity = 2000m, UnitPrice = 45m, TaxCode = gst5Code, TaxRatePercent = 5m, DisplayOrder = 1 },
                    new() { ItemName = "Pallet wrap rolls", Quantity = 100m, UnitPrice = 320m, TaxCode = gst5Code, TaxRatePercent = 5m, DisplayOrder = 2 }
                }
            },
            new PurchaseOrder
            {
                Organization = org, PoNumber = "PO-00003", Vendor = steelVendor, Status = "Received",
                OrderDate = DateTime.UtcNow.Date.AddDays(-20), Owner = managerUser,
                Lines = new List<PurchaseOrderLine>
                {
                    new() { ItemName = "Structural steel beams", Product = steelBeamProduct, Quantity = 50m, UnitPrice = 12500m, TaxCode = gst18Code, TaxRatePercent = 18m, DisplayOrder = 1 }
                }
            }
        );

        // Opening stock balances for the demo org's products (Phase 4 — Inventory), seeded as
        // standalone Receipt movements with no EntityType/EntityId (manual/initial stock load,
        // same as a real "record adjustment" action would produce). All loaded into the
        // Bengaluru warehouse (Phase 4 — Warehouse retrofit), demonstrating WarehouseId against
        // real data the same way ProductId was retrofitted onto pre-existing order lines above.
        var stockLoadDate = DateTime.UtcNow.Date.AddDays(-30);
        db.StockMovements.AddRange(
            new StockMovement { Organization = org, Product = steelSheetProduct, Warehouse = bengaluruWarehouse, MovementType = "Receipt", Quantity = 2000m, MovementDate = stockLoadDate, Notes = "Opening stock" },
            new StockMovement { Organization = org, Product = steelBeamProduct, Warehouse = bengaluruWarehouse, MovementType = "Receipt", Quantity = 150m, MovementDate = stockLoadDate, Notes = "Opening stock" },
            new StockMovement { Organization = org, Product = posTerminalProduct, Warehouse = bengaluruWarehouse, MovementType = "Receipt", Quantity = 25m, MovementDate = stockLoadDate, Notes = "Opening stock" },
            new StockMovement { Organization = org, Product = cartonProduct, Warehouse = bengaluruWarehouse, MovementType = "Receipt", Quantity = 5000m, MovementDate = stockLoadDate, Notes = "Opening stock" }
        );

        // Demo stock transfer (Phase 4 — Warehouse): 200 units of STEEL-2MM moved from
        // Bengaluru to Pune. The two resulting ledger movements need the transfer's own Id for
        // EntityId, which isn't available until after the row is actually saved — same
        // two-phase-save reasoning as SalesOrder.OrderNumber, just done here in seed data
        // instead of inside a service.
        var demoTransfer = new StockTransfer
        {
            Organization = org, Product = steelSheetProduct,
            FromWarehouse = bengaluruWarehouse, ToWarehouse = puneWarehouse,
            Quantity = 200m, TransferDate = DateTime.UtcNow.Date.AddDays(-10), Notes = "Restock Pune for regional orders"
        };
        db.StockTransfers.Add(demoTransfer);

        var transferIssueMovement = new StockMovement { Organization = org, Product = steelSheetProduct, Warehouse = bengaluruWarehouse, MovementType = "Issue", Quantity = -200m, MovementDate = demoTransfer.TransferDate };
        var transferReceiptMovement = new StockMovement { Organization = org, Product = steelSheetProduct, Warehouse = puneWarehouse, MovementType = "Receipt", Quantity = 200m, MovementDate = demoTransfer.TransferDate };
        db.StockMovements.AddRange(transferIssueMovement, transferReceiptMovement);

        // Demo finished good + bill of materials (Phase 4 — Manufacturing). None of the four
        // raw-material-style Products above represents an assembly, so a new finished-good
        // Product is needed to give a BOM something to build.
        var rackProduct = new Product { Organization = org, Sku = "RACK-ASM", Name = "Steel Storage Rack (Assembled)", UnitOfMeasure = "EA", UnitCost = 6200m, IsActive = true };
        db.Products.Add(rackProduct);

        var rackBom = new BillOfMaterial
        {
            Organization = org, Product = rackProduct, IsActive = true,
            Components = new List<BomComponent>
            {
                new() { ComponentProduct = steelSheetProduct, Quantity = 4m, DisplayOrder = 1 },
                new() { ComponentProduct = steelBeamProduct, Quantity = 2m, DisplayOrder = 2 }
            }
        };
        db.BillOfMaterials.Add(rackBom);

        // Seed data bypasses ProductionOrderService so MoNumber is set explicitly here (same
        // reason as SalesOrder.OrderNumber/PurchaseOrder.PoNumber above). Left Draft so it can
        // be completed live during verification — 10 racks need 40 sheets + 20 beams, well
        // within the Bengaluru warehouse's on-hand after the demo transfer above (1,800 sheets,
        // 150 beams).
        db.ProductionOrders.Add(new ProductionOrder
        {
            Organization = org, MoNumber = "MO-00001", Product = rackProduct, Warehouse = bengaluruWarehouse,
            Quantity = 10m, Status = "Draft", OrderDate = DateTime.UtcNow.Date, Owner = managerUser
        });

        // Basic chart of accounts for the demo org (Phase 5 — General Ledger).
        var cashAccount = new LedgerAccount { Organization = org, Code = "1000", Name = "Cash", Type = "Asset", IsActive = true };
        var arAccount = new LedgerAccount { Organization = org, Code = "1100", Name = "Accounts Receivable", Type = "Asset", IsActive = true };
        var inventoryAssetAccount = new LedgerAccount { Organization = org, Code = "1200", Name = "Inventory Asset", Type = "Asset", IsActive = true };
        var apAccount = new LedgerAccount { Organization = org, Code = "2000", Name = "Accounts Payable", Type = "Liability", IsActive = true };
        var equityAccount = new LedgerAccount { Organization = org, Code = "3000", Name = "Owner's Equity", Type = "Equity", IsActive = true };
        var revenueAccount = new LedgerAccount { Organization = org, Code = "4000", Name = "Sales Revenue", Type = "Revenue", IsActive = true };
        var cogsAccount = new LedgerAccount { Organization = org, Code = "5000", Name = "Cost of Goods Sold", Type = "Expense", IsActive = true };
        var interestIncomeAccount = new LedgerAccount { Organization = org, Code = "4100", Name = "Interest Income", Type = "Revenue", IsActive = true };
        // Added for the Payroll module (Phase 6) — Salaries Payable holds withheld deductions
        // until remitted (out of scope), Salary Expense is debited by every PayRun.Pay.
        var salaryExpenseAccount = new LedgerAccount { Organization = org, Code = "5100", Name = "Salary Expense", Type = "Expense", IsActive = true };
        var salariesPayableAccount = new LedgerAccount { Organization = org, Code = "2100", Name = "Salaries Payable", Type = "Liability", IsActive = true };
        db.LedgerAccounts.AddRange(cashAccount, arAccount, inventoryAssetAccount, apAccount, equityAccount, revenueAccount, cogsAccount, interestIncomeAccount, salaryExpenseAccount, salariesPayableAccount);

        // Seed data bypasses JournalEntryService so EntryNumber is set explicitly here (same
        // reason as SalesOrder.OrderNumber/ProductionOrder.MoNumber above). One entry is already
        // Posted (opening balances render immediately on load), the other left Draft so it can
        // be posted live during verification.
        var openingCashLine = new JournalEntryLine { LedgerAccount = cashAccount, Debit = 500000m, Credit = 0m, DisplayOrder = 1 };
        var openingBalancesEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00001", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-30), Description = "Opening balances", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                openingCashLine,
                new() { LedgerAccount = equityAccount, Debit = 0m, Credit = 500000m, DisplayOrder = 2 }
            }
        };
        db.JournalEntries.AddRange(
            openingBalancesEntry,
            new JournalEntry
            {
                Organization = org, EntryNumber = "JE-00002", Status = "Draft",
                EntryDate = DateTime.UtcNow.Date, Description = "Record a customer sale", Owner = employeeUser,
                Lines = new List<JournalEntryLine>
                {
                    new() { LedgerAccount = arAccount, Debit = 100000m, Credit = 0m, DisplayOrder = 1 },
                    new() { LedgerAccount = revenueAccount, Debit = 0m, Credit = 100000m, DisplayOrder = 2 }
                }
            }
        );

        // Demo vendor bills (Phase 5 — Accounts Payable). One is already Approved with its own
        // posted JournalEntry seeded directly and linked via the PostedJournalEntry nav
        // property — EF resolves the FK automatically at save time, no two-phase Id patch
        // needed (unlike the polymorphic EntityType/EntityId used by StockMovement/StockTransfer,
        // this is a real typed FK). The other is left Draft for live approval during verification.
        var steelBillEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00003", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-15), Description = "Vendor Bill BILL-00001", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = inventoryAssetAccount, Debit = 425000m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = apAccount, Debit = 0m, Credit = 425000m, DisplayOrder = 2 }
            }
        };
        db.JournalEntries.Add(steelBillEntry);

        db.VendorBills.AddRange(
            new VendorBill
            {
                Organization = org, BillNumber = "BILL-00001", Vendor = steelVendor, PayableLedgerAccount = apAccount,
                BillDate = DateTime.UtcNow.Date.AddDays(-15), DueDate = DateTime.UtcNow.Date.AddDays(15),
                Status = "Approved", Owner = managerUser, PostedJournalEntry = steelBillEntry,
                Lines = new List<VendorBillLine>
                {
                    new() { LedgerAccount = inventoryAssetAccount, Description = "Steel sheets delivery", Amount = 425000m, DisplayOrder = 1 }
                }
            },
            new VendorBill
            {
                Organization = org, BillNumber = "BILL-00002", Vendor = packagingVendor, PayableLedgerAccount = apAccount,
                BillDate = DateTime.UtcNow.Date, DueDate = DateTime.UtcNow.Date.AddDays(30),
                Status = "Draft", Owner = employeeUser,
                Lines = new List<VendorBillLine>
                {
                    new() { LedgerAccount = cogsAccount, Description = "Packaging materials", Amount = 64100m, DisplayOrder = 1 }
                }
            }
        );

        // Demo customer invoices (Phase 5 — Accounts Receivable), mirroring the VendorBill
        // seed above with Debit/Credit reversed. One already Sent with its own posted
        // JournalEntry linked via the PostedJournalEntry nav property — the 189,000 amount
        // ties back narratively to the already-seeded SO-00002 Globex order total. The other
        // is left Draft for live Send during verification.
        var globexInvoiceEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00004", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-7), Description = "Customer Invoice INV-00001", Owner = employeeUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = arAccount, Debit = 189000m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = revenueAccount, Debit = 0m, Credit = 189000m, DisplayOrder = 2 }
            }
        };
        db.JournalEntries.Add(globexInvoiceEntry);

        db.CustomerInvoices.AddRange(
            new CustomerInvoice
            {
                Organization = org, InvoiceNumber = "INV-00001", Account = globexAccount, ReceivableLedgerAccount = arAccount,
                InvoiceDate = DateTime.UtcNow.Date.AddDays(-7), DueDate = DateTime.UtcNow.Date.AddDays(23),
                Status = "Sent", Owner = employeeUser, PostedJournalEntry = globexInvoiceEntry,
                Lines = new List<CustomerInvoiceLine>
                {
                    new() { LedgerAccount = revenueAccount, Description = "POS Terminal Hardware order", Amount = 189000m, DisplayOrder = 1 }
                }
            },
            new CustomerInvoice
            {
                Organization = org, InvoiceNumber = "INV-00002", Account = acmeAccount, ReceivableLedgerAccount = arAccount,
                InvoiceDate = DateTime.UtcNow.Date, DueDate = DateTime.UtcNow.Date.AddDays(30),
                Status = "Draft", Owner = managerUser,
                Lines = new List<CustomerInvoiceLine>
                {
                    new() { LedgerAccount = revenueAccount, Description = "Consulting services", Amount = 50000m, DisplayOrder = 1 }
                }
            }
        );

        // Demo bank reconciliation (Phase 5 — Bank Reconciliation, closing out the phase). A
        // bank-only transaction (interest earned) demonstrates a case Sales/Purchase/AP/AR
        // postings wouldn't otherwise produce.
        var interestCashLine = new JournalEntryLine { LedgerAccount = cashAccount, Debit = 5000m, Credit = 0m, DisplayOrder = 1 };
        db.JournalEntries.Add(new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00005", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-1), Description = "Bank interest earned", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                interestCashLine,
                new() { LedgerAccount = interestIncomeAccount, Debit = 0m, Credit = 5000m, DisplayOrder = 2 }
            }
        });

        // One Completed reconciliation (opening deposit already matched) and one Draft left
        // for live matching + completing during verification. Cash's book balance at
        // fresh-seed time is 500,000 (JE-00001) + 5,000 (JE-00005) - 246,600 (JE-00006, the
        // Payroll module's PR-00001 Paid net-pay credit) + 180 (the POS module's net effect:
        // JE-00007's +180 from POS-00002 Completed, and JE-00008/JE-00009 netting to 0 for
        // POS-00003's Completed-then-Refunded pair) = 258,580 — the seeded
        // VendorBill/CustomerInvoice are Approved/Sent, not yet Paid, so they don't affect Cash.
        db.BankReconciliations.AddRange(
            new BankReconciliation
            {
                Organization = org, LedgerAccount = cashAccount, StatementDate = DateTime.UtcNow.Date.AddDays(-30),
                StatementEndingBalance = 500000m, Status = "Completed", Owner = managerUser,
                Lines = new List<BankStatementLine>
                {
                    new() { TransactionDate = DateTime.UtcNow.Date.AddDays(-30), Description = "Opening deposit", Amount = 500000m, MatchedJournalEntryLine = openingCashLine, DisplayOrder = 1 }
                }
            },
            new BankReconciliation
            {
                Organization = org, LedgerAccount = cashAccount, StatementDate = DateTime.UtcNow.Date,
                StatementEndingBalance = 258580m, Status = "Draft", Owner = managerUser,
                Lines = new List<BankStatementLine>
                {
                    new() { TransactionDate = DateTime.UtcNow.Date.AddDays(-1), Description = "Bank interest credited", Amount = 5000m, DisplayOrder = 1 }
                }
            }
        );

        // Demo shipments (Warehouse module extension). One SalesOrder-sourced shipment fulfills
        // the already-seeded Confirmed SO-00002 (Globex, 10x POS Terminal Hardware) — left Open
        // so picking/shipping it live during verification demonstrates the relocated stock-deduction
        // point (Confirm no longer touches stock; Ship does). The other is a TransferOrder-
        // sourced shipment already Shipped (Bengaluru -> Pune, 200x cartons), with its own
        // Issue/Receipt movements seeded directly and linked via the same two-phase EntityId
        // patch already used for demoTransfer's movements above (Shipment's EntityType/EntityId
        // are plain ints, not a real FK, so the shipment's own Id isn't available until saved).
        var salesOrderShipment = new Shipment
        {
            Organization = org, ShipmentNumber = "SHIP-00001", Warehouse = bengaluruWarehouse, SourceType = "SalesOrder",
            SalesOrder = globexSalesOrder, Status = "Open", Owner = employeeUser,
            ShipToName = "Globex Retail — Receiving Dock", ShipToAddressLine1 = "48 Commerce Way",
            ShipToCity = "Mumbai", ShipToState = "Maharashtra", ShipToPostalCode = "400001", ShipToCountry = "India",
            ShipDate = DateTime.UtcNow.Date, Carrier = "BlueDart", IsBlindShipment = false,
            Lines = new List<ShipmentLine>
            {
                new() { Product = posTerminalProduct, Quantity = 10m, SalesOrderLine = globexPosTerminalLine, DisplayOrder = 1 }
            },
            Packages = new List<ShipmentPackage>
            {
                new() { PackageNumber = 1, WeightKg = 45.5m, LengthCm = 60m, WidthCm = 40m, HeightCm = 35m }
            }
        };

        var transferShipment = new Shipment
        {
            Organization = org, ShipmentNumber = "SHIP-00002", Warehouse = bengaluruWarehouse, SourceType = "TransferOrder",
            DestinationWarehouse = puneWarehouse, Status = "Shipped", Owner = managerUser,
            ShipDate = DateTime.UtcNow.Date.AddDays(-3), Carrier = "VRL Logistics", TrackingNumber = "VRL-887744",
            IsBlindShipment = true,
            Lines = new List<ShipmentLine>
            {
                new() { Product = cartonProduct, Quantity = 200m, DisplayOrder = 1 }
            },
            Packages = new List<ShipmentPackage>
            {
                new() { PackageNumber = 1, WeightKg = 120m, LengthCm = 80m, WidthCm = 60m, HeightCm = 60m, TrackingNumber = "VRL-887744-1" }
            }
        };
        db.Shipments.AddRange(salesOrderShipment, transferShipment);

        var shipmentTransferIssue = new StockMovement { Organization = org, Product = cartonProduct, Warehouse = bengaluruWarehouse, MovementType = "Issue", Quantity = -200m, MovementDate = transferShipment.ShipDate };
        var shipmentTransferReceipt = new StockMovement { Organization = org, Product = cartonProduct, Warehouse = puneWarehouse, MovementType = "Receipt", Quantity = 200m, MovementDate = transferShipment.ShipDate };
        db.StockMovements.AddRange(shipmentTransferIssue, shipmentTransferReceipt);

        // HRMS (Phase 6, first module). EmployeeCode is assigned as a literal string here
        // (not via the usual two-phase Id-patch scheme) — seed data has no live Id to build
        // from before the first save, and every Employee would otherwise default to the same
        // empty EmployeeCode and collide on the unique index the moment more than one is
        // saved in the same batch (same fix already applied to the Shipment module's seed).
        // sivaManager is seeded as priyaEmployee's ReportingManagerId to exercise the
        // self-referencing hierarchy, and linked via UserId to the already-seeded managerUser
        // to demonstrate the optional Employee<->User link.
        var sivaManager = new Employee
        {
            Organization = org, EmployeeCode = "EMP-00001", Department = engineeringDept, User = managerUser,
            FirstName = "Siva", LastName = "Bangaru", Email = "siva.bangaru@novaerp.local",
            Phone = "+91-9876500010", JobTitle = "Engineering Manager", EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date.AddYears(-3), Status = "Active"
        };
        var priyaEmployee = new Employee
        {
            // Linked to employeeUser (employee@novaerp.local) so the Employee Portal module
            // (Phase 6, third module) has real self-service data to demonstrate live: her own
            // Pending leave request and Paid payslip line, already seeded below.
            Organization = org, EmployeeCode = "EMP-00002", Department = engineeringDept, ReportingManager = sivaManager,
            User = employeeUser,
            FirstName = "Priya", LastName = "Sharma", Email = "priya.sharma@novaerp.local",
            Phone = "+91-9876500011", JobTitle = "Software Engineer", EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date.AddYears(-1), Status = "Active"
        };
        var rameshEmployee = new Employee
        {
            Organization = org, EmployeeCode = "EMP-00003", Department = financeDept,
            FirstName = "Ramesh", LastName = "Iyer", Email = "ramesh.iyer@novaerp.local",
            Phone = "+91-9876500012", JobTitle = "Accounts Executive", EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date.AddMonths(-8), Status = "Active"
        };
        var kavyaEmployee = new Employee
        {
            Organization = org, EmployeeCode = "EMP-00004", Department = salesDept,
            FirstName = "Kavya", LastName = "Reddy", Email = "kavya.reddy@novaerp.local",
            Phone = "+91-9876500013", JobTitle = "Sales Associate", EmploymentType = "Contract",
            DateOfJoining = DateTime.UtcNow.Date.AddMonths(-2), Status = "Active"
        };
        // Linked to adminUser so admin@novaerp.local can also exercise self-service flows that
        // require a linked Employee (e.g. the AI Assistant's create-Service-Ticket action),
        // not just Manager/Employee.
        var adminEmployee = new Employee
        {
            Organization = org, EmployeeCode = "EMP-00005", Department = engineeringDept, User = adminUser,
            FirstName = "NovaERP", LastName = "Admin", Email = "admin@novaerp.local",
            Phone = "+91-9876500009", JobTitle = "System Administrator", EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date.AddYears(-4), Status = "Active"
        };
        db.Employees.AddRange(sivaManager, priyaEmployee, rameshEmployee, kavyaEmployee, adminEmployee);

        var annualLeave = new LeaveType { Organization = org, Name = "Annual Leave", Code = "ANNUAL", DefaultDaysPerYear = 18m, IsActive = true };
        var sickLeave   = new LeaveType { Organization = org, Name = "Sick Leave", Code = "SICK", DefaultDaysPerYear = 10m, IsActive = true };
        var unpaidLeave = new LeaveType { Organization = org, Name = "Unpaid Leave", Code = "UNPAID", DefaultDaysPerYear = null, IsActive = true };
        db.LeaveTypes.AddRange(annualLeave, sickLeave, unpaidLeave);

        var pendingLeaveRequest = new LeaveRequest
        {
            Organization = org, Employee = priyaEmployee, LeaveType = annualLeave,
            StartDate = DateTime.UtcNow.Date.AddDays(10), EndDate = DateTime.UtcNow.Date.AddDays(14),
            DaysRequested = 5m, Reason = "Family function", Status = "Pending"
        };
        var approvedLeaveRequest = new LeaveRequest
        {
            Organization = org, Employee = rameshEmployee, LeaveType = sickLeave,
            StartDate = DateTime.UtcNow.Date.AddDays(-5), EndDate = DateTime.UtcNow.Date.AddDays(-4),
            DaysRequested = 2m, Reason = "Fever", Status = "Approved"
        };
        db.LeaveRequests.AddRange(pendingLeaveRequest, approvedLeaveRequest);

        // Payroll (Phase 6, second module). EmployeeCompensation is seeded for all 4 existing
        // Employees so ProcessAsync has real compensation to snapshot from during verification.
        var sivaComp = new EmployeeCompensation { Organization = org, Employee = sivaManager, BasicSalary = 80000m, Hra = 32000m, OtherAllowances = 8000m, Deductions = 12000m };
        var priyaComp = new EmployeeCompensation { Organization = org, Employee = priyaEmployee, BasicSalary = 50000m, Hra = 20000m, OtherAllowances = 5000m, Deductions = 7500m };
        var rameshComp = new EmployeeCompensation { Organization = org, Employee = rameshEmployee, BasicSalary = 35000m, Hra = 14000m, OtherAllowances = 3000m, Deductions = 5200m };
        var kavyaComp = new EmployeeCompensation { Organization = org, Employee = kavyaEmployee, BasicSalary = 25000m, Hra = 0m, OtherAllowances = 2000m, Deductions = 2700m };
        db.EmployeeCompensations.AddRange(sivaComp, priyaComp, rameshComp, kavyaComp);

        // RunNumber is assigned as a literal here (not via the usual two-phase Id-patch
        // scheme) for the same reason as Employee.EmployeeCode above — seed data has no live
        // Id before the first save, and two PayRuns saved in the same batch would otherwise
        // collide on the unique index.
        var payrollJournalEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00006", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddMonths(-1), Description = "Pay Run PR-00001", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = salaryExpenseAccount, Debit = 274000m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = salariesPayableAccount, Debit = 0m, Credit = 27400m, DisplayOrder = 2 },
                new() { LedgerAccount = cashAccount, Debit = 0m, Credit = 246600m, DisplayOrder = 3 }
            }
        };
        db.JournalEntries.Add(payrollJournalEntry);

        var lastMonthPayRun = new PayRun
        {
            Organization = org, RunNumber = "PR-00001",
            PeriodMonth = DateTime.UtcNow.Date.AddMonths(-1).Month, PeriodYear = DateTime.UtcNow.Date.AddMonths(-1).Year,
            ExpenseLedgerAccount = salaryExpenseAccount, DeductionsPayableLedgerAccount = salariesPayableAccount,
            Status = "Paid", Owner = managerUser, PostedJournalEntry = payrollJournalEntry,
            Lines = new List<PayRunLine>
            {
                new() { Employee = sivaManager, BasicSalary = 80000m, Hra = 32000m, OtherAllowances = 8000m, Deductions = 12000m, GrossPay = 120000m, NetPay = 108000m },
                new() { Employee = priyaEmployee, BasicSalary = 50000m, Hra = 20000m, OtherAllowances = 5000m, Deductions = 7500m, GrossPay = 75000m, NetPay = 67500m },
                new() { Employee = rameshEmployee, BasicSalary = 35000m, Hra = 14000m, OtherAllowances = 3000m, Deductions = 5200m, GrossPay = 52000m, NetPay = 46800m },
                new() { Employee = kavyaEmployee, BasicSalary = 25000m, Hra = 0m, OtherAllowances = 2000m, Deductions = 2700m, GrossPay = 27000m, NetPay = 24300m }
            }
        };
        var currentMonthPayRun = new PayRun
        {
            Organization = org, RunNumber = "PR-00002",
            PeriodMonth = DateTime.UtcNow.Date.Month, PeriodYear = DateTime.UtcNow.Date.Year,
            ExpenseLedgerAccount = salaryExpenseAccount, DeductionsPayableLedgerAccount = salariesPayableAccount,
            Status = "Draft", Owner = managerUser
        };
        db.PayRuns.AddRange(lastMonthPayRun, currentMonthPayRun);

        // Projects (Phase 7, first module). One project per manager-eligible Employee for
        // variety; ProjectTasks span all three with varied Status/Priority, one assigned to
        // Priya Sharma (continuity with the Phase 6 seed data/portal story) and one
        // deliberately left unassigned.
        var erpProject = new Project
        {
            Organization = org, Code = "PROJ-ERP", Name = "ERP Rollout", Manager = sivaManager,
            Description = "Company-wide NovaERP adoption", StartDate = DateTime.UtcNow.Date.AddMonths(-2),
            EndDate = DateTime.UtcNow.Date.AddMonths(4), Budget = 2500000m, Status = "Active"
        };
        var warehouseProject = new Project
        {
            Organization = org, Code = "PROJ-WMS", Name = "Warehouse Automation", Manager = rameshEmployee,
            Description = "Conveyor + barcode scanning rollout at the Pune warehouse", StartDate = DateTime.UtcNow.Date.AddMonths(1),
            Budget = 900000m, Status = "Planning"
        };
        var retailProject = new Project
        {
            Organization = org, Code = "PROJ-RETAIL", Name = "Retail Expansion", Manager = kavyaEmployee,
            Description = "New storefront market research and rollout", StartDate = DateTime.UtcNow.Date.AddMonths(-1),
            Status = "OnHold"
        };
        db.Projects.AddRange(erpProject, warehouseProject, retailProject);

        db.ProjectTasks.AddRange(
            new ProjectTask { Organization = org, Project = erpProject, AssignedTo = priyaEmployee, Title = "Design API contracts", Priority = "High", Status = "InProgress", DueDate = DateTime.UtcNow.Date.AddDays(7) },
            new ProjectTask { Organization = org, Project = erpProject, AssignedTo = sivaManager, Title = "Set up CI pipeline", Priority = "Medium", Status = "Done", DueDate = DateTime.UtcNow.Date.AddDays(-3) },
            new ProjectTask { Organization = org, Project = erpProject, Title = "Write onboarding docs", Priority = "Low", Status = "ToDo", DueDate = DateTime.UtcNow.Date.AddDays(21) },
            new ProjectTask { Organization = org, Project = warehouseProject, AssignedTo = rameshEmployee, Title = "Vendor evaluation", Priority = "High", Status = "InProgress", DueDate = DateTime.UtcNow.Date.AddDays(14) },
            new ProjectTask { Organization = org, Project = warehouseProject, AssignedTo = kavyaEmployee, Title = "Budget approval", Priority = "Medium", Status = "Blocked", DueDate = DateTime.UtcNow.Date.AddDays(10) },
            new ProjectTask { Organization = org, Project = retailProject, AssignedTo = priyaEmployee, Title = "Market research", Priority = "Low", Status = "ToDo", DueDate = DateTime.UtcNow.Date.AddDays(30) }
        );

        // Assets (Phase 7, second module). AssetCode is assigned as a literal here (not via
        // the usual two-phase Id-patch scheme) for the same reason as Employee.EmployeeCode/
        // Shipment.ShipmentNumber above — seed data has no live Id before the first save, and
        // multiple Assets saved in the same batch would otherwise collide on the unique index.
        var laptopCategory = new AssetCategory { Organization = org, Name = "Laptop", Code = "LAPTOP", IsActive = true };
        var vehicleCategory = new AssetCategory { Organization = org, Name = "Vehicle", Code = "VEHICLE", IsActive = true };
        var furnitureCategory = new AssetCategory { Organization = org, Name = "Furniture", Code = "FURNITURE", IsActive = true };
        db.AssetCategories.AddRange(laptopCategory, vehicleCategory, furnitureCategory);

        db.Assets.AddRange(
            new Asset { Organization = org, AssetCode = "AST-00001", Name = "Dell Latitude 5420", Category = laptopCategory, SerialNumber = "DL5420-001", PurchaseDate = DateTime.UtcNow.Date.AddYears(-1), PurchaseCost = 85000m, WarrantyExpiryDate = DateTime.UtcNow.Date.AddYears(2), AssignedTo = sivaManager, Status = "Assigned" },
            new Asset { Organization = org, AssetCode = "AST-00002", Name = "MacBook Pro 14", Category = laptopCategory, SerialNumber = "MBP14-002", PurchaseDate = DateTime.UtcNow.Date.AddMonths(-6), PurchaseCost = 180000m, WarrantyExpiryDate = DateTime.UtcNow.Date.AddMonths(30), AssignedTo = priyaEmployee, Status = "Assigned" },
            new Asset { Organization = org, AssetCode = "AST-00003", Name = "Lenovo ThinkPad T14", Category = laptopCategory, SerialNumber = "TP14-003", PurchaseDate = DateTime.UtcNow.Date.AddMonths(-2), PurchaseCost = 95000m, WarrantyExpiryDate = DateTime.UtcNow.Date.AddYears(3), Status = "Available" },
            new Asset { Organization = org, AssetCode = "AST-00004", Name = "Toyota Innova (Fleet)", Category = vehicleCategory, SerialNumber = "KA-01-AB-1234", PurchaseDate = DateTime.UtcNow.Date.AddYears(-2), PurchaseCost = 1450000m, Status = "UnderMaintenance" },
            new Asset { Organization = org, AssetCode = "AST-00005", Name = "Office Desk (Old)", Category = furnitureCategory, PurchaseDate = DateTime.UtcNow.Date.AddYears(-5), PurchaseCost = 12000m, Status = "Retired" }
        );

        // Service Desk (Phase 7, third module). TicketNumber is assigned as a literal here (not
        // via the usual two-phase Id-patch scheme) for the same reason as Asset.AssetCode above.
        var hardwareCategory = new TicketCategory { Organization = org, Name = "Hardware", Code = "HARDWARE", IsActive = true };
        var softwareCategory = new TicketCategory { Organization = org, Name = "Software", Code = "SOFTWARE", IsActive = true };
        var accessCategory = new TicketCategory { Organization = org, Name = "Access", Code = "ACCESS", IsActive = true };
        db.TicketCategories.AddRange(hardwareCategory, softwareCategory, accessCategory);

        db.ServiceTickets.AddRange(
            new ServiceTicket { Organization = org, TicketNumber = "TCK-00001", Subject = "Laptop screen flickering", Description = "Screen flickers intermittently since this morning.", Category = hardwareCategory, Requester = priyaEmployee, Priority = "Medium", Status = "Open" },
            new ServiceTicket { Organization = org, TicketNumber = "TCK-00002", Subject = "VPN client fails to connect", Description = "VPN client throws a timeout error when connecting from home network.", Category = softwareCategory, Requester = rameshEmployee, AssignedTo = priyaEmployee, Priority = "High", Status = "InProgress" },
            new ServiceTicket { Organization = org, TicketNumber = "TCK-00003", Subject = "Need access to Finance shared drive", Description = "Requesting read access to the Finance shared drive for month-end reporting.", Category = accessCategory, Requester = kavyaEmployee, AssignedTo = sivaManager, Priority = "Medium", Status = "Resolved", ResolutionNotes = "Access granted via AD group FinanceReadOnly.", ResolvedDate = DateTime.UtcNow.Date.AddDays(-2) },
            new ServiceTicket { Organization = org, TicketNumber = "TCK-00004", Subject = "Replace worn keyboard", Description = "Several keys have stopped registering presses.", Category = hardwareCategory, Requester = sivaManager, AssignedTo = rameshEmployee, Priority = "Low", Status = "Closed", ResolutionNotes = "Keyboard replaced from spares stock.", ResolvedDate = DateTime.UtcNow.Date.AddDays(-6), ClosedDate = DateTime.UtcNow.Date.AddDays(-5) },
            new ServiceTicket { Organization = org, TicketNumber = "TCK-00005", Subject = "Production database connection outage", Description = "Application servers cannot reach the production database.", Category = softwareCategory, Requester = priyaEmployee, Priority = "Critical", Status = "Open" }
        );

        // POS (Phase 8, first Channel module). SaleNumber is assigned as a literal here (not
        // via the usual two-phase Id-patch scheme) for the same reason as
        // ServiceTicket.TicketNumber/Asset.AssetCode above. Two sales already have their own
        // posted JournalEntry seeded directly and linked via the PostedJournalEntry nav
        // property, same pattern as VendorBill/CustomerInvoice/PayRun above — the third is
        // left Draft for live Complete during verification.
        var posSaleCompletedEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00007", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-1), Description = "POS Sale POS-00002", Owner = employeeUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = cashAccount, Debit = 180m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = revenueAccount, Debit = 0m, Credit = 180m, DisplayOrder = 2 }
            }
        };
        var posSaleRefundedEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00008", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-3), Description = "POS Sale POS-00003", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = cashAccount, Debit = 900m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = revenueAccount, Debit = 0m, Credit = 900m, DisplayOrder = 2 }
            }
        };
        // The refund's own reversing entry — nets POS-00003's cash/revenue effect to zero,
        // same "two posted entries for one now-reversed transaction" shape RefundAsync itself
        // produces live.
        var posSaleRefundReversalEntry = new JournalEntry
        {
            Organization = org, EntryNumber = "JE-00009", Status = "Posted",
            EntryDate = DateTime.UtcNow.Date.AddDays(-2), Description = "Refund for POS Sale POS-00003", Owner = managerUser,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccount = revenueAccount, Debit = 900m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccount = cashAccount, Debit = 0m, Credit = 900m, DisplayOrder = 2 }
            }
        };
        db.JournalEntries.AddRange(posSaleCompletedEntry, posSaleRefundedEntry, posSaleRefundReversalEntry);

        db.PosSales.AddRange(
            new PosSale
            {
                Organization = org, SaleNumber = "POS-00001", Warehouse = bengaluruWarehouse, RevenueLedgerAccount = revenueAccount,
                SaleDate = DateTime.UtcNow.Date, Owner = employeeUser, Status = "Draft",
                Lines = new List<PosSaleLine> { new() { Product = posTerminalProduct, Quantity = 2m, UnitPrice = 19500m } }
            },
            new PosSale
            {
                Organization = org, SaleNumber = "POS-00002", Warehouse = bengaluruWarehouse, CustomerAccount = acmeAccount,
                RevenueLedgerAccount = revenueAccount, PaymentLedgerAccount = cashAccount,
                SaleDate = DateTime.UtcNow.Date.AddDays(-1), Owner = employeeUser, Status = "Completed",
                PostedJournalEntry = posSaleCompletedEntry,
                Lines = new List<PosSaleLine> { new() { Product = cartonProduct, Quantity = 3m, UnitPrice = 60m } }
            },
            new PosSale
            {
                Organization = org, SaleNumber = "POS-00003", Warehouse = bengaluruWarehouse, CustomerAccount = globexAccount,
                RevenueLedgerAccount = revenueAccount, PaymentLedgerAccount = cashAccount,
                SaleDate = DateTime.UtcNow.Date.AddDays(-3), Owner = managerUser, Status = "Refunded",
                PostedJournalEntry = posSaleRefundedEntry,
                Lines = new List<PosSaleLine> { new() { Product = steelSheetProduct, Quantity = 1m, UnitPrice = 900m } }
            }
        );

        db.StockMovements.AddRange(
            new StockMovement { Organization = org, Product = cartonProduct, Warehouse = bengaluruWarehouse, MovementType = "Issue", Quantity = -3m, MovementDate = DateTime.UtcNow.Date.AddDays(-1), Notes = "POS Sale POS-00002" },
            new StockMovement { Organization = org, Product = steelSheetProduct, Warehouse = bengaluruWarehouse, MovementType = "Issue", Quantity = -1m, MovementDate = DateTime.UtcNow.Date.AddDays(-3), Notes = "POS Sale POS-00003" },
            new StockMovement { Organization = org, Product = steelSheetProduct, Warehouse = bengaluruWarehouse, MovementType = "Receipt", Quantity = 1m, MovementDate = DateTime.UtcNow.Date.AddDays(-2), Notes = "Refund for POS Sale POS-00003" }
        );

        // Retail (Phase 8, second module) — plain reference data, no lifecycle, same shape as
        // Warehouse/AssetCategory/TicketCategory before it.
        db.Stores.AddRange(
            new Store { Organization = org, Branch = headOffice, Warehouse = bengaluruWarehouse, Name = "Bengaluru Flagship Store", Code = "STORE-BLR", Address = "Bagmane Tech Park, Bengaluru", IsActive = true },
            new Store { Organization = org, Branch = puneBranch, Warehouse = puneWarehouse, Name = "Pune Regional Store", Code = "STORE-PUN", Address = "Hinjewadi Phase 2, Pune", IsActive = true }
        );

        // Languages — system-wide reference data.
        var english = new Language { Code = "en", Name = "English", NativeName = "English", IsRtl = false, IsActive = true };
        var hindi   = new Language { Code = "hi", Name = "Hindi", NativeName = "हिन्दी", IsRtl = false, IsActive = true };
        var french  = new Language { Code = "fr", Name = "French", NativeName = "Français", IsRtl = false, IsActive = true };
        var arabic  = new Language { Code = "ar", Name = "Arabic", NativeName = "العربية", IsRtl = true, IsActive = true };
        var spanish = new Language { Code = "es", Name = "Spanish", NativeName = "Español", IsRtl = false, IsActive = true };
        db.Languages.AddRange(english, hindi, french, arabic, spanish);

        // Demo org supports English (default) + Hindi.
        db.OrganizationLanguages.AddRange(
            new OrganizationLanguage { Organization = org, LanguageCode = "en", IsDefault = true },
            new OrganizationLanguage { Organization = org, LanguageCode = "hi", IsDefault = false }
        );

        // Approval workflow engine — one demo tiered definition. Under 10,000 INR the Manager
        // role approves; at or above that, the Admin role approves instead. No live
        // WorkflowInstance is seeded here — StartAsync is exercised by tests instead.
        var expenseClaimWorkflow = new WorkflowDefinition
        {
            Organization = org,
            Name = "Expense Claim Approval",
            EntityType = "ExpenseClaim",
            IsActive = true,
            Steps = new List<WorkflowStepDefinition>
            {
                new() { StepOrder = 1, Name = "Manager Approval", ApproverRoleId = managerRole.Id, MinAmount = null },
                new() { StepOrder = 2, Name = "Admin Approval (High Value)", ApproverRoleId = adminRole.Id, MinAmount = 10000m }
            }
        };
        db.WorkflowDefinitions.Add(expenseClaimWorkflow);

        // Workflow automation — notify the Admin role whenever any workflow is fully approved.
        db.AutomationRules.Add(new AutomationRule
        {
            Organization = org,
            Name = "Notify Admin on workflow approval",
            TriggerEvent = AutomationEvents.WorkflowApproved,
            ActionType = "Notify",
            NotifyRoleId = adminRole.Id,
            NotifyMessageTemplate = "Workflow '{EntityType}' #{EntityId} was approved.",
            IsEnabled = true
        });

        // Scheduler — demo org's two recurring jobs, both enabled out of the box.
        db.ScheduledJobDefinitions.AddRange(
            new ScheduledJobDefinition
            {
                Organization = org,
                Name = "Approval Reminder",
                JobKey = "ApprovalReminder",
                CronExpression = "0 9 * * *",
                IsEnabled = true
            },
            new ScheduledJobDefinition
            {
                Organization = org,
                Name = "Stale Exchange Rate Check",
                JobKey = "StaleExchangeRateCheck",
                CronExpression = "0 8 * * 1",
                IsEnabled = true
            }
        );

        // Notification channel settings — one row for the demo org with every provider field
        // left null/empty. This honestly represents the real default "not configured yet" state;
        // no fake SMTP/SMS/push credentials are seeded.
        db.NotificationChannelSettings.Add(new NotificationChannelSettings
        {
            Organization = org,
            SmtpUseSsl = true
        });

        await db.SaveChangesAsync();

        // Second phase: now that demoTransfer has its own Id, link the two movements it
        // produced back to it (same two-phase-save reasoning noted where it's constructed above).
        transferIssueMovement.EntityType = "StockTransfer";
        transferIssueMovement.EntityId = demoTransfer.Id;
        transferReceiptMovement.EntityType = "StockTransfer";
        transferReceiptMovement.EntityId = demoTransfer.Id;

        // Same reasoning for the demo TransferOrder-sourced Shipment's own movements.
        shipmentTransferIssue.EntityType = "Shipment";
        shipmentTransferIssue.EntityId = transferShipment.Id;
        shipmentTransferReceipt.EntityType = "Shipment";
        shipmentTransferReceipt.EntityId = transferShipment.Id;

        // Dashboard Builder (Phase 9, first module). Owned by employeeUser, not a Role — every
        // authenticated user manages their own dashboards. 3 of the 4 widget types are
        // pre-added at mixed sizes; the 4th (PosSalesTotal) is left available to add live
        // during verification.
        db.Dashboards.Add(new Dashboard
        {
            Organization = org, User = employeeUser, Name = "My Dashboard", IsDefault = true,
            Widgets = new List<DashboardWidget>
            {
                new() { WidgetType = "SalesOrderStatusSummary", Title = "Sales Orders by Status", SizeOption = "Medium", DisplayOrder = 1 },
                new() { WidgetType = "ServiceTicketStatusSummary", Title = "Tickets by Status", SizeOption = "Small", DisplayOrder = 2 },
                new() { WidgetType = "ProjectTaskStatusSummary", Title = "Project Tasks by Status", SizeOption = "Large", DisplayOrder = 3 }
            }
        });

        await db.SaveChangesAsync();
    }
}
