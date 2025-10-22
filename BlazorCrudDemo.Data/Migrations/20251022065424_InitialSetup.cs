using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorCrudDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "590cee40-b147-4a4a-a007-b41e14a56b2c", new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(2810), "AQAAAAIAAYagAAAAEE2uK5AbB2/Nc/0g5TVe+sy+cwdSeQZe59MA4h5Ap/73KLYyYZRDWg+m2xS76T/JBA==", "109a2371-750a-4eed-80b3-8e86a6604767" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8690), new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8690) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8720), new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8720) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8730), new DateTime(2025, 10, 22, 6, 54, 16, 939, DateTimeKind.Utc).AddTicks(8730) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(680), new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(690) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(700), new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(700) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(710), new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(710) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(710), new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(710) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(720), new DateTime(2025, 10, 22, 6, 54, 16, 940, DateTimeKind.Utc).AddTicks(720) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "042ba3e8-9ec8-4607-bf46-1fa5117f6376", new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(6040), "AQAAAAIAAYagAAAAEJSXjUbcqHZWTi/snv58FueQEbNRQ8wp1Q12bXAoMzyYjCImOIHOLfdKgjq5lXSE7Q==", "91be0b3c-173d-444e-a88d-194d7b2016ec" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4730), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4740) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4740), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4740) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4750), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(4750) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5500), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5500) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5510), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5510) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5510), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5510) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5520), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5520) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5530), new DateTime(2025, 10, 22, 6, 49, 35, 391, DateTimeKind.Utc).AddTicks(5530) });
        }
    }
}
