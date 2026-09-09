using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PromotionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Coupon"),
                    DiscountType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Percentage"),
                    DiscountValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    MaxDiscount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UsageLimitPerCustomer = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HappyHourStart = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    HappyHourEnd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    HappyHourDays = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApplicableItemIds = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BuyQty = table.Column<int>(type: "integer", nullable: true),
                    GetQty = table.Column<int>(type: "integer", nullable: true),
                    GiftCardBalance = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    GiftCardUsed = table.Column<decimal>(type: "numeric(10,2)", nullable: true, defaultValue: 0m),
                    SpendThreshold = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PointsMultiplierVal = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BannerColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BadgeIcon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                table: "Promotions",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_IsActive",
                table: "Promotions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_OrganizationId",
                table: "Promotions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_PromotionType",
                table: "Promotions",
                column: "PromotionType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promotions");
        }
    }
}
