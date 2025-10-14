# BlazorCrudDemo Database Setup

This document provides instructions for setting up and managing the Entity Framework Core database for the BlazorCrudDemo application.

## Database Configuration

The application uses SQLite as the database provider. The connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blazorcrud.db"
  }
}
```

## Database Features

- **Audit Fields**: Automatic `CreatedDate` and `ModifiedDate` tracking
- **Soft Delete**: Records are marked as inactive instead of being deleted
- **Seed Data**: Initial categories and products are automatically created
- **Indexes**: Optimized indexes for performance
- **Constraints**: Database-level constraints for data integrity

## Migration Commands

### Initial Setup

1. **Create Initial Migration** (if not exists):
```bash
cd BlazorCrudDemo.Web
dotnet ef migrations add InitialCreate --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

2. **Apply Migrations**:
```bash
cd BlazorCrudDemo.Web
dotnet ef database update --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

### Creating New Migrations

When you make changes to your models:

1. **Add Migration**:
```bash
cd BlazorCrudDemo.Web
dotnet ef migrations add [MigrationName] --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

2. **Update Database**:
```bash
cd BlazorCrudDemo.Web
dotnet ef database update --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

### Migration Management

- **List Migrations**:
```bash
cd BlazorCrudDemo.Web
dotnet ef migrations list --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

- **Remove Last Migration**:
```bash
cd BlazorCrudDemo.Web
dotnet ef migrations remove --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

- **Generate SQL Script**:
```bash
cd BlazorCrudDemo.Web
dotnet ef migrations script --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

## Database Schema

### Tables

#### Categories
- `Id` (INTEGER PRIMARY KEY)
- `Name` (TEXT, NOT NULL, MAX 100 chars)
- `Description` (TEXT, MAX 500 chars)
- `Icon` (TEXT, MAX 200 chars)
- `DisplayOrder` (INTEGER, NOT NULL, DEFAULT 0)
- `CreatedDate` (TEXT, NOT NULL)
- `ModifiedDate` (TEXT, NOT NULL)
- `IsActive` (INTEGER, NOT NULL, DEFAULT 1)

#### Products
- `Id` (INTEGER PRIMARY KEY)
- `Name` (TEXT, NOT NULL, MAX 200 chars)
- `Description` (TEXT, MAX 1000 chars)
- `Price` (DECIMAL(18,2), NOT NULL, > 0)
- `Stock` (INTEGER, NOT NULL, >= 0)
- `SKU` (TEXT, NOT NULL, MAX 50 chars, UNIQUE)
- `ImageUrl` (TEXT, MAX 500 chars)
- `CategoryId` (INTEGER, NOT NULL, FK to Categories)
- `CreatedDate` (TEXT, NOT NULL)
- `ModifiedDate` (TEXT, NOT NULL)
- `IsActive` (INTEGER, NOT NULL, DEFAULT 1)

### Indexes

#### Categories
- `IX_Categories_Name` (UNIQUE on Name)
- `IX_Categories_DisplayOrder`
- `IX_Categories_IsActive`

#### Products
- `IX_Products_SKU` (UNIQUE on SKU)
- `IX_Products_Name`
- `IX_Products_CategoryId`
- `IX_Products_IsActive`
- `IX_Products_CategoryId_IsActive` (Composite)

### Constraints

#### Categories
- `CK_Categories_DisplayOrder_NonNegative`: DisplayOrder >= 0

#### Products
- `CK_Products_Price_Positive`: Price > 0
- `CK_Products_Stock_NonNegative`: Stock >= 0

## Seed Data

The database is automatically seeded with:

### Categories (3)
1. Electronics (DisplayOrder: 1)
2. Books (DisplayOrder: 2)
3. Clothing (DisplayOrder: 3)

### Products (5)
1. MacBook Pro 16-inch (Electronics)
2. iPhone 15 Pro (Electronics)
3. Clean Code (Books)
4. Design Patterns (Books)
5. Cotton T-Shirt (Clothing)

## Development Workflow

1. Make model changes in the `BlazorCrudDemo.Shared/Models/` directory
2. Update configurations in `BlazorCrudDemo.Data/Configurations/`
3. Create and apply migrations using the commands above
4. Test your changes

## Troubleshooting

### Common Issues

1. **Migration Errors**: Ensure all projects are built before creating migrations
2. **Database Locked**: Close any applications using the SQLite file
3. **Foreign Key Violations**: Check cascade delete rules and soft delete implementation

### Reset Database

To completely reset the database:

```bash
# Delete the database file
rm BlazorCrudDemo.Web/blazorcrud.db

# Run the application to recreate with seed data
cd BlazorCrudDemo.Web
dotnet run
```

## Production Considerations

For production deployment:

1. Use a production-grade database (SQL Server, PostgreSQL, etc.)
2. Update connection strings in `appsettings.Production.json`
3. Run migrations during deployment
4. Consider using database migrations in CI/CD pipelines
5. Set up proper backup strategies
