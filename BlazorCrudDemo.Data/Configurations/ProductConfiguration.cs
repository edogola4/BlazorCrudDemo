using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table name
            builder.ToTable("Products");

            // Primary key
            builder.HasKey(p => p.Id);

            // Properties configuration
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Name");

            builder.Property(p => p.Description)
                .HasMaxLength(1000)
                .HasColumnName("Description");

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasColumnName("Price");

            builder.Property(p => p.Stock)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("Stock");

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("SKU");

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageUrl");

            builder.Property(p => p.CategoryId)
                .IsRequired()
                .HasColumnName("CategoryId");

            // Audit fields
            builder.Property(p => p.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("CreatedDate");

            builder.Property(p => p.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("ModifiedDate");

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

            // Indexes for performance
            builder.HasIndex(p => p.SKU)
                .IsUnique()
                .HasDatabaseName("IX_Products_SKU");

            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive");

            // Composite index for common queries
            builder.HasIndex(p => new { p.CategoryId, p.IsActive })
                .HasDatabaseName("IX_Products_CategoryId_IsActive");

            // Check constraints
            builder.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
            builder.HasCheckConstraint("CK_Products_Stock_NonNegative", "[Stock] >= 0");

            // Foreign key relationship (configured in context for cascade behavior)
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_Products_Categories");

            // Computed columns (if needed for database-level calculations)
            // Note: These are handled as regular properties in the model
        }
    }
}
