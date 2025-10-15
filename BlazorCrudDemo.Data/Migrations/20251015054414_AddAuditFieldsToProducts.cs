using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlazorCrudDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_DisplayOrder_NonNegative", "[DisplayOrder] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
                    table.CheckConstraint("CK_Products_Stock_NonNegative", "[Stock] >= 0");
                    table.ForeignKey(
                        name: "FK_Products_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedDate", "Description", "DisplayOrder", "Icon", "IsActive", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5880), "Electronic devices and accessories", 1, "fas fa-laptop", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5880), "Electronics" },
                    { 2, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5890), "Books and educational materials", 2, "fas fa-book", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(5890), "Books" },
                    { 3, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6040), "Fashion and apparel items", 3, "fas fa-tshirt", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6040), "Clothing" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedDate", "Description", "ImageUrl", "IsActive", "ModifiedDate", "Name", "Price", "SKU", "Stock" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6300), "High-performance laptop for professionals", "https://example.com/macbook-pro.jpg", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6300), "MacBook Pro 16-inch", 2399.99m, "MBP16-001", 15 },
                    { 2, 1, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6310), "Latest smartphone with advanced camera system", "https://example.com/iphone-15-pro.jpg", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6310), "iPhone 15 Pro", 999.99m, "IPH15P-001", 25 },
                    { 3, 2, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6320), "A Handbook of Agile Software Craftsmanship", "https://example.com/clean-code.jpg", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6320), "Clean Code", 39.99m, "CC-BOOK-001", 50 },
                    { 4, 2, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), "Elements of Reusable Object-Oriented Software", "https://example.com/design-patterns.jpg", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), "Design Patterns", 49.99m, "DP-BOOK-001", 30 },
                    { 5, 3, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), "Comfortable 100% cotton t-shirt", "https://example.com/cotton-tshirt.jpg", true, new DateTime(2025, 10, 15, 5, 44, 12, 928, DateTimeKind.Utc).AddTicks(6330), "Cotton T-Shirt", 19.99m, "TSHIRT-COT-001", 100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
