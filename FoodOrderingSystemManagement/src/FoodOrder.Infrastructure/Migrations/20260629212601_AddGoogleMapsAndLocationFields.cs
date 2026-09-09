using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleMapsAndLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RestaurantLatitude",
                table: "Settings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RestaurantLongitude",
                table: "Settings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentLatitude",
                table: "Drivers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentLongitude",
                table: "Drivers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdate",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryLatitude",
                table: "DeliveryOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryLongitude",
                table: "DeliveryOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GoogleMapsApiKey", "RestaurantLatitude", "RestaurantLongitude" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleMapsApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RestaurantLatitude",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RestaurantLongitude",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DeliveryLatitude",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryLongitude",
                table: "DeliveryOrders");
        }
    }
}
