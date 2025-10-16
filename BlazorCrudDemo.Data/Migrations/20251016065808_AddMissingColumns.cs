using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorCrudDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(900), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(900) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(910), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(910) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(920), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(920) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1420), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1430) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1430), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1440) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1460), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1460) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1470), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1470) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1480), new DateTime(2025, 10, 16, 6, 58, 3, 953, DateTimeKind.Utc).AddTicks(1480) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5510), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5510) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5520), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5520) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5530), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5530) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5540), new DateTime(2025, 10, 16, 6, 55, 19, 806, DateTimeKind.Utc).AddTicks(5550) });
        }
    }
}
