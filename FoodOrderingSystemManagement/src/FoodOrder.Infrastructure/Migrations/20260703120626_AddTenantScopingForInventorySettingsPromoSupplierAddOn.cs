using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopingForInventorySettingsPromoSupplierAddOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "SupplierPayments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "StockTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "InventoryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "AddOns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // ── Backfill ────────────────────────────────────────────────────────
            // All pre-existing rows in these tables belonged operationally to
            // TOP 10 (Organization Id 1 / its default Branch Id 1) — the only org
            // with real data before tenant scoping existed here.
            migrationBuilder.Sql(@"
                UPDATE ""Suppliers"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" = 0;
                UPDATE ""SupplierPayments"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" = 0;
                UPDATE ""PurchaseOrders"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" = 0;
                UPDATE ""AddOns"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" = 0;
                UPDATE ""InventoryItems"" SET ""BranchId"" = 1 WHERE ""BranchId"" = 0;
                UPDATE ""StockTransactions"" SET ""BranchId"" = 1 WHERE ""BranchId"" = 0;

                -- Settings.OrganizationId and Promotions.OrganizationId already existed as
                -- nullable columns but were never actually populated (every row is NULL),
                -- which is the root cause of the reported cross-tenant leak — the new
                -- AppDbContext query filter compares against the caller's real org id, so a
                -- NULL row would become invisible to everyone except SuperAdmin otherwise.
                UPDATE ""Settings"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" IS NULL;
                UPDATE ""Promotions"" SET ""OrganizationId"" = 1 WHERE ""OrganizationId"" IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_OrganizationId",
                table: "Suppliers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_OrganizationId",
                table: "SupplierPayments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_BranchId",
                table: "StockTransactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OrganizationId",
                table: "PurchaseOrders",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_BranchId",
                table: "InventoryItems",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOns_OrganizationId",
                table: "AddOns",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_Organizations_OrganizationId",
                table: "AddOns",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Branches_BranchId",
                table: "InventoryItems",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Organizations_OrganizationId",
                table: "PurchaseOrders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Branches_BranchId",
                table: "StockTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPayments_Organizations_OrganizationId",
                table: "SupplierPayments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Organizations_OrganizationId",
                table: "Suppliers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_Organizations_OrganizationId",
                table: "AddOns");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Branches_BranchId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Organizations_OrganizationId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Branches_BranchId",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPayments_Organizations_OrganizationId",
                table: "SupplierPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Organizations_OrganizationId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_OrganizationId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayments_OrganizationId",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_BranchId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_OrganizationId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_BranchId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AddOns_OrganizationId",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AddOns");
        }
    }
}
