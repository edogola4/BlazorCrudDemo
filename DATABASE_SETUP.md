# BlazorCrudDemo Database Setup

This document provides comprehensive instructions for setting up and managing the Entity Framework Core database for the BlazorCrudDemo application, including authentication, audit logging, and all feature tables.

## Database Configuration

The application uses SQLite as the default database provider with automatic database creation and migrations. The connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blazorcrud.db"
  }
}
```

## Database Features

- **Audit Fields**: Automatic `CreatedDate`, `ModifiedDate`, and user tracking
- **Soft Delete**: Records are marked as inactive instead of being deleted
- **Seed Data**: Initial categories, products, and admin user are automatically created
- **Indexes**: Optimized indexes for performance and search functionality
- **Constraints**: Database-level constraints for data integrity
- **Identity Integration**: Full ASP.NET Core Identity support with roles and claims
- **Audit Logging**: Comprehensive activity tracking for compliance

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

## Complete Database Schema

### Core Tables

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

### Authentication Tables (ASP.NET Core Identity)

#### Users
- `Id` (TEXT PRIMARY KEY)
- `UserName` (TEXT, UNIQUE)
- `NormalizedUserName` (TEXT, UNIQUE)
- `Email` (TEXT, UNIQUE)
- `NormalizedEmail` (TEXT, UNIQUE)
- `EmailConfirmed` (INTEGER, NOT NULL)
- `PasswordHash` (TEXT)
- `SecurityStamp` (TEXT)
- `ConcurrencyStamp` (TEXT)
- `PhoneNumber` (TEXT)
- `PhoneNumberConfirmed` (INTEGER, NOT NULL)
- `TwoFactorEnabled` (INTEGER, NOT NULL)
- `LockoutEnd` (TEXT)
- `LockoutEnabled` (INTEGER, NOT NULL)
- `AccessFailedCount` (INTEGER, NOT NULL)

#### Roles
- `Id` (TEXT PRIMARY KEY)
- `Name` (TEXT, UNIQUE)
- `NormalizedName` (TEXT, UNIQUE)
- `ConcurrencyStamp` (TEXT)

#### UserRoles
- `UserId` (TEXT, NOT NULL, FK to Users)
- `RoleId` (TEXT, NOT NULL, FK to Roles)

#### UserClaims
- `Id` (INTEGER PRIMARY KEY)
- `UserId` (TEXT, NOT NULL, FK to Users)
- `ClaimType` (TEXT)
- `ClaimValue` (TEXT)

#### UserLogins
- `LoginProvider` (TEXT, NOT NULL)
- `ProviderKey` (TEXT, NOT NULL)
- `ProviderDisplayName` (TEXT)
- `UserId` (TEXT, NOT NULL, FK to Users)

#### UserTokens
- `UserId` (TEXT, NOT NULL, FK to Users)
- `LoginProvider` (TEXT, NOT NULL)
- `Name` (TEXT, NOT NULL)
- `Value` (TEXT)

#### RoleClaims
- `Id` (INTEGER PRIMARY KEY)
- `RoleId` (TEXT, NOT NULL, FK to Roles)
- `ClaimType` (TEXT)
- `ClaimValue` (TEXT)

### Audit & Logging Tables

#### AuditLogs
- `Id` (INTEGER PRIMARY KEY)
- `UserId` (TEXT, FK to Users)
- `Action` (TEXT, NOT NULL) - The action performed (Create, Update, Delete, Login, etc.)
- `EntityName` (TEXT, NOT NULL) - The entity type affected
- `EntityId` (TEXT) - The ID of the affected entity
- `OldValues` (TEXT) - JSON representation of old values
- `NewValues` (TEXT) - JSON representation of new values
- `IpAddress` (TEXT) - IP address of the user
- `UserAgent` (TEXT) - Browser/client user agent
- `Timestamp` (TEXT, NOT NULL) - When the action occurred
- `Changes` (TEXT) - Detailed description of changes made

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

#### Identity Tables
- `IX_Users_NormalizedUserName` (UNIQUE)
- `IX_Users_NormalizedEmail` (UNIQUE)
- `IX_Roles_NormalizedName` (UNIQUE)
- `IX_UserRoles_UserId_RoleId` (Composite)
- `IX_UserClaims_UserId`
- `IX_UserLogins_UserId_LoginProvider_ProviderKey` (Composite)
- `IX_UserTokens_UserId_LoginProvider_Name` (Composite)
- `IX_RoleClaims_RoleId`

#### Audit Tables
- `IX_AuditLogs_UserId`
- `IX_AuditLogs_Timestamp`
- `IX_AuditLogs_EntityName_EntityId` (Composite)
- `IX_AuditLogs_Action`

### Database Constraints

#### Categories
- `CK_Categories_DisplayOrder_NonNegative`: DisplayOrder >= 0

#### Products
- `CK_Products_Price_Positive`: Price > 0
- `CK_Products_Stock_NonNegative`: Stock >= 0

#### Identity Constraints
- `CK_Users_EmailConfirmed_Boolean`: EmailConfirmed IN (0, 1)
- `CK_Users_PhoneNumberConfirmed_Boolean`: PhoneNumberConfirmed IN (0, 1)
- `CK_Users_TwoFactorEnabled_Boolean`: TwoFactorEnabled IN (0, 1)
- `CK_Users_LockoutEnabled_Boolean`: LockoutEnabled IN (0, 1)
- `CK_Users_AccessFailedCount_NonNegative`: AccessFailedCount >= 0

## Seed Data

The database is automatically seeded with comprehensive initial data:

### Default Admin User
- **Email**: admin@demo.com
- **Username**: admin@demo.com
- **Password**: Admin123!
- **Role**: Administrator

### Categories (3)
1. **Electronics** (DisplayOrder: 1)
   - Icon: bi-cpu
   - Description: Electronic devices and gadgets
2. **Books** (DisplayOrder: 2)
   - Icon: bi-book
   - Description: Books and educational materials
3. **Clothing** (DisplayOrder: 3)
   - Icon: bi-shirt
   - Description: Apparel and accessories

### Products (5)
1. **MacBook Pro 16-inch** (Electronics)
   - Price: $2,399.00
   - Stock: 15
   - SKU: MBP16-001
2. **iPhone 15 Pro** (Electronics)
   - Price: $999.00
   - Stock: 30
   - SKU: IP15P-001
3. **Clean Code** (Books)
   - Price: $45.99
   - Stock: 50
   - SKU: BK-CC-001
4. **Design Patterns** (Books)
   - Price: $39.99
   - Stock: 35
   - SKU: BK-DP-001
5. **Cotton T-Shirt** (Clothing)
   - Price: $19.99
   - Stock: 100
   - SKU: TSH-COT-001

## Development Workflow

1. **Model Changes**: Make changes in the `BlazorCrudDemo.Shared/Models/` directory
2. **Update Configurations**: Modify entity configurations in `BlazorCrudDemo.Data/Configurations/`
3. **Create Migrations**: Generate migrations using the commands above
4. **Apply Migrations**: Update database schema with new migrations
5. **Test Changes**: Verify functionality and data integrity
6. **Seed Data**: Ensure seed data is updated if new entities are added

## Troubleshooting

### Common Issues

1. **Migration Errors**:
   - Ensure all projects are built before creating migrations
   - Check for compilation errors in the solution
   - Verify Entity Framework Core tools are installed

2. **Database Locked**:
   - Close any applications using the SQLite file
   - Ensure no other instances of the application are running
   - Check file permissions on the database file

3. **Foreign Key Violations**:
   - Review cascade delete rules in entity configurations
   - Check soft delete implementation and constraints
   - Verify referential integrity in seed data

4. **Identity Migration Issues**:
   - ASP.NET Core Identity tables are managed automatically
   - Avoid manual modifications to identity-related migrations
   - Use Identity UI or services for user management

5. **Audit Logging Errors**:
   - Ensure audit interceptor is properly registered in DbContext
   - Check JSON serialization settings for audit data
   - Verify timestamp and user context availability

### Database Reset

To completely reset the database:

```bash
# Stop the application
# Delete the database file
rm BlazorCrudDemo.Web/blazorcrud.db
rm BlazorCrudDemo.Web/blazorcrud.db-shm
rm BlazorCrudDemo.Web/blazorcrud.db-wal

# Run the application to recreate with seed data
cd BlazorCrudDemo.Web
dotnet run
```

### Migration Issues

If you encounter migration problems:

```bash
# Clear migration history and recreate
cd BlazorCrudDemo.Web
dotnet ef database drop --force --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
dotnet ef migrations add InitialCreate --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
dotnet ef database update --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

## Production Considerations

For production deployment:

### Database Selection
1. **Primary Database**: Use production-grade databases (SQL Server, PostgreSQL, MySQL)
2. **Connection Strings**: Configure in `appsettings.Production.json`
3. **Migration Strategy**: Run migrations during deployment pipeline
4. **Backup Strategy**: Implement automated backup and recovery procedures

### Identity Configuration
```json
{
  "IdentityOptions": {
    "Password": {
      "RequiredLength": 8,
      "RequireNonAlphanumeric": true,
      "RequireUppercase": true,
      "RequireLowercase": true
    },
    "Lockout": {
      "DefaultLockoutTimeSpan": "00:05:00",
      "MaxFailedAccessAttempts": 5
    }
  }
}
```

### Audit and Compliance
- Configure audit logging retention policies
- Set up audit data archiving for long-term storage
- Implement data export capabilities for compliance reporting
- Configure audit trail access controls

### Performance Optimization
- **Indexes**: Add additional indexes based on query patterns
- **Connection Pooling**: Configure optimal connection pool settings
- **Query Optimization**: Monitor and optimize frequently executed queries
- **Partitioning**: Consider table partitioning for large datasets

### Migration Strategy for Production
```bash
# Production migration commands
cd BlazorCrudDemo.Web
dotnet ef migrations bundle --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
dotnet ef database update --project ../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj --startup-project BlazorCrudDemo.Web.csproj
```

### Monitoring and Maintenance
- **Database Health**: Monitor connection counts, query performance, and error rates
- **Audit Log Analysis**: Set up monitoring for audit trail integrity
- **Automated Cleanup**: Configure background services for log rotation and cleanup
- **Alert Configuration**: Set up alerts for database issues and security events

## Security Considerations

### Authentication Security
- Enable HTTPS and security headers in production
- Configure secure password policies
- Implement account lockout after failed attempts
- Use secure random token generation for password resets

### Data Protection
- Encrypt sensitive data at rest and in transit
- Implement proper access controls for audit logs
- Configure data retention policies
- Regular security audits and penetration testing

### Audit Compliance
- Ensure audit logs are tamper-proof
- Implement audit trail integrity checking
- Configure audit data backup and recovery
- Regular review of audit policies and procedures
