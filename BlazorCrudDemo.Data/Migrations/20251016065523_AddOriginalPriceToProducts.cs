using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorCrudDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalPriceToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4610), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4610) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4620), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4620) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4630), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(4630) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate", "OriginalPrice", "Rating", "ReviewCount", "Tags" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5510), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5510), 0m, 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate", "OriginalPrice", "Rating", "ReviewCount", "Tags" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5520), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5520), 0m, 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate", "OriginalPrice", "Rating", "ReviewCount", "Tags" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5530), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5530), 0m, 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate", "OriginalPrice", "Rating", "ReviewCount", "Tags" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540), 0m, 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate", "OriginalPrice", "Rating", "ReviewCount", "Tags" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5550), 0m, 0m, 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5880), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5880) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5890), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5890) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6040), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6040) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6300), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6300) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6310), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6310) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6320), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6320) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330) });
        }
    }
}
