# Authentication System - Quick Start

## Overview

This document provides a quick start guide for the BlazorCrudDemo authentication system. For comprehensive documentation, see [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md).

## Features

✅ **Dual Authentication**: Cookie-based (Blazor Server) + JWT (API)  
✅ **User Management**: Registration, login, logout, password reset  
✅ **Security**: Account lockout, password hashing, token expiration  
✅ **Authorization**: Role-based (Admin/User) and policy-based  
✅ **Audit Logging**: Complete tracking of authentication events  
✅ **Session Management**: Refresh tokens, sliding expiration  

## Quick Start

### 1. Default Accounts

The system comes with pre-configured test accounts:

**Admin Account**
```
Email: admin@blazorcrud.com
Password: Admin123!
```

**User Account**
```
Email: user@blazorcrud.com
Password: User123!
```

### 2. Login via Web Interface

1. Navigate to `https://localhost:5120/auth/login`
2. Enter credentials
3. Click "Log in"

### 3. Login via API

```bash
curl -X POST https://localhost:5120/api/auth/login \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "admin@blazorcrud.com",
    "password": "Admin123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "xMzI0NTY3ODkw...",
  "expiresAt": "2024-11-04T07:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@blazorcrud.com",
    "roles": ["Admin"]
  }
}
```

### 4. Use JWT Token

```bash
TOKEN="your-access-token-here"

curl -X GET https://localhost:5120/api/auth/me \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

## Architecture

```
┌─────────────────────────────────────────┐
│         Client (Browser/API)            │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  Controllers (Account/Auth)             │
│  - Cookie Auth (Blazor pages)           │
│  - JWT Auth (API endpoints)             │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  AuthenticationService                  │
│  - Login/Register/Logout                │
│  - Token generation                     │
│  - Password management                  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  ASP.NET Core Identity                  │
│  - User management                      │
│  - Password hashing                     │
│  - Role management                      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  Database (SQLite)                      │
│  - Users, Roles, Claims                 │
│  - LoginHistory, AuditLogs              │
└─────────────────────────────────────────┘
```

## Key Components

### 1. AuthenticationService
**Location:** `BlazorCrudDemo.Web/Services/AuthenticationService.cs`

Core service handling all authentication logic:
- User login/logout
- JWT token generation
- Refresh token management
- Password operations
- Audit logging

### 2. Controllers

**AccountController** (`Controllers/AccountController.cs`)
- Cookie-based authentication for Blazor Server
- Form-based login/logout
- Password reset pages

**AuthController** (`Controllers/AuthController.cs`)
- RESTful API endpoints
- JWT token authentication
- JSON request/response

### 3. Authentication State Provider
**Location:** `BlazorCrudDemo.Web/Services/CustomAuthenticationStateProvider.cs`

Manages authentication state for Blazor components:
- Tracks current user
- Validates user status
- Notifies components of auth changes

### 4. Data Models

**ApplicationUser** (`BlazorCrudDemo.Data/Models/ApplicationUser.cs`)
```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    // ... audit fields
}
```

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | Login and get JWT token | No |
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/logout` | Logout current user | Yes |
| POST | `/api/auth/refresh-token` | Refresh JWT token | No |
| GET | `/api/auth/me` | Get current user info | Yes |
| GET | `/api/auth/test-jwt` | Test JWT authentication | Yes |

### Web Pages

| Route | Description | Auth Required |
|-------|-------------|---------------|
| `/auth/login` | Login page | No |
| `/auth/register` | Registration page | No |
| `/Account/ForgotPassword` | Password reset request | No |
| `/Account/ResetPassword` | Password reset confirmation | No |

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/blazorcrud.db"
  },
  "Jwt": {
    "Key": "your-secret-key-here-at-least-32-characters",
    "Issuer": "https://localhost:5120",
    "Audience": "https://localhost:5120"
  }
}
```

### Identity Options (Program.cs)

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
});
```

## Security Features

### 1. Password Security
- **Hashing**: PBKDF2 with HMAC-SHA256
- **Salt**: 128-bit random salt per password
- **Iterations**: 10,000+ (configurable)
- **Requirements**: 8+ chars, uppercase, lowercase, digit, special char

### 2. Account Lockout
- **Trigger**: 5 failed login attempts
- **Duration**: 30 minutes
- **Logging**: All attempts recorded

### 3. JWT Security
- **Algorithm**: HMAC-SHA256
- **Key Size**: 256+ bits
- **Expiration**: 1 hour (access token)
- **Refresh**: Cryptographically random 32-byte token

### 4. Cookie Security
- **HttpOnly**: Yes (prevents XSS)
- **Secure**: Yes (HTTPS only in production)
- **SameSite**: Lax (CSRF protection)
- **Expiration**: 2 hours with sliding

## Usage Examples

### Register New User (C#)

```csharp
@inject IAuthenticationService AuthService

private async Task RegisterUser()
{
    var registerDto = new RegisterDto
    {
        Email = "newuser@example.com",
        Password = "SecurePass123!",
        ConfirmPassword = "SecurePass123!",
        FirstName = "John",
        LastName = "Doe",
        Role = "User"
    };
    
    var result = await AuthService.RegisterAsync(registerDto);
    
    if (result.Success)
    {
        // Registration successful
        Console.WriteLine($"User created: {result.User.Email}");
    }
    else
    {
        // Handle errors
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
    }
}
```

### Login User (JavaScript)

```javascript
async function login(email, password) {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });
    
    const result = await response.json();
    
    if (result.success) {
        localStorage.setItem('accessToken', result.accessToken);
        localStorage.setItem('refreshToken', result.refreshToken);
        return result.user;
    } else {
        throw new Error(result.message);
    }
}
```

### Protect Blazor Page

```razor
@page "/admin/dashboard"
@attribute [Authorize(Roles = "Admin")]

<h3>Admin Dashboard</h3>
<p>Only administrators can see this page.</p>
```

### Protect API Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class ProductsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "UserOnly")] // Requires User or Admin role
    public async Task<IActionResult> GetProducts()
    {
        // Implementation
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")] // Requires Admin role only
    public async Task<IActionResult> CreateProduct()
    {
        // Implementation
    }
}
```

## Testing

### Quick Test Commands

```bash
# Test registration
curl -X POST https://localhost:5120/api/auth/register \
  -H "Content-Type: application/json" \
  -k \
  -d '{"email":"test@example.com","password":"Test123!","confirmPassword":"Test123!","firstName":"Test","lastName":"User","role":"User"}'

# Test login
curl -X POST https://localhost:5120/api/auth/login \
  -H "Content-Type: application/json" \
  -k \
  -d '{"email":"admin@blazorcrud.com","password":"Admin123!"}'

# Test protected endpoint (replace TOKEN)
curl -X GET https://localhost:5120/api/auth/me \
  -H "Authorization: Bearer TOKEN" \
  -k
```

For comprehensive testing guide, see [AUTHENTICATION_TESTING.md](AUTHENTICATION_TESTING.md).

## Troubleshooting

### Common Issues

**"Invalid email or password"**
- Check credentials are correct
- Verify account is active (`IsActive = true`)
- Check if account is locked out

**"401 Unauthorized" on API calls**
- Verify token is included in Authorization header
- Check token hasn't expired
- Ensure Bearer prefix is included

**Database errors**
- Run migrations: `dotnet ef database update`
- Check connection string in appsettings.json
- Verify database file permissions

**Account locked**
- Wait 30 minutes or manually unlock in database:
```sql
UPDATE AspNetUsers SET LockoutEnd = NULL WHERE Email = 'user@example.com';
```

## Documentation

- **[AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md)** - Comprehensive guide with architecture, API reference, and security details
- **[AUTHENTICATION_TESTING.md](AUTHENTICATION_TESTING.md)** - Complete testing guide with examples and scenarios
- **[PROJECT_EXPLANATION.md](PROJECT_EXPLANATION.md)** - Overall project documentation

## Support

For issues or questions:
1. Check the comprehensive guides above
2. Review the inline code documentation
3. Check application logs in `logs/` directory
4. Examine database tables for audit trail

---

**Version:** 1.0.0  
**Last Updated:** November 4, 2024
