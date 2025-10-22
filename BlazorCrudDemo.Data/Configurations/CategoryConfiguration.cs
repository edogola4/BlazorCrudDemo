using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Table name
            builder.ToTable("Categories");

            // Primary key
            builder.HasKey(c => c.Id);

            // Properties configuration
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Name");

            builder.Property(c => c.Description)
                .HasMaxLength(500)
                .HasColumnName("Description");

            builder.Property(c => c.Icon)
                .HasMaxLength(200)
                .HasColumnName("Icon");

            builder.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("DisplayOrder");

            // Audit fields
            builder.Property(c => c.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("CreatedDate");

            builder.Property(c => c.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("ModifiedDate");

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

            // Indexes for performance
            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");

            builder.HasIndex(c => c.DisplayOrder)
                .HasDatabaseName("IX_Categories_DisplayOrder");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Categories_IsActive");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_Categories_DisplayOrder_NonNegative", "[DisplayOrder] >= 0"));

            // Relationship back-reference (Products collection)
            // This is configured in the Product entity configuration

            // Seed data will be handled in the DbContext
        }
    }
}
