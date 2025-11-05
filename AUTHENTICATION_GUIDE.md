# Authentication System - Complete Guide

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Components](#components)
4. [Authentication Flow](#authentication-flow)
5. [API Reference](#api-reference)
6. [Security Features](#security-features)
7. [Usage Examples](#usage-examples)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The BlazorCrudDemo application implements a **hybrid authentication system** that supports both:
- **Cookie-based authentication** for Blazor Server pages
- **JWT (JSON Web Token) authentication** for API endpoints

This dual approach provides flexibility for different client types while maintaining security and user experience.

### Key Features
✅ User registration and login  
✅ JWT token generation and validation  
✅ Refresh token support  
✅ Role-based authorization (Admin, User)  
✅ Policy-based authorization  
✅ Account lockout protection  
✅ Password reset functionality  
✅ Audit logging for all authentication events  
✅ Session management  
✅ Secure cookie handling  

---

## Architecture

### Authentication Stack
```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                          │
│  - Login/Register Pages (Blazor)                        │
│  - API Clients (HTTP/JavaScript)                        │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  Controller Layer                        │
│  - AccountController (Cookie Auth)                      │
│  - AuthController (JWT Auth)                            │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   Service Layer                          │
│  - IAuthenticationService                               │
│  - AuthenticationService (Business Logic)               │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  Identity Layer                          │
│  - ASP.NET Core Identity                                │
│  - UserManager<ApplicationUser>                         │
│  - SignInManager<ApplicationUser>                       │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   Data Layer                             │
│  - ApplicationDbContext                                 │
│  - SQLite Database                                      │
│  - ApplicationUser, LoginHistory, AuditLog              │
└─────────────────────────────────────────────────────────┘
```

### Project Structure
```
BlazorCrudDemo/
├── BlazorCrudDemo.Data/
│   ├── Models/
│   │   ├── ApplicationUser.cs          # Extended IdentityUser
│   │   ├── LoginHistory.cs             # Login tracking
│   │   └── AuditLog.cs                 # Audit trail
│   └── Contexts/
│       └── ApplicationDbContext.cs     # EF Core context
├── BlazorCrudDemo.Shared/
│   └── DTOs/
│       └── AuthDtos.cs                 # Data transfer objects
└── BlazorCrudDemo.Web/
    ├── Controllers/
    │   ├── AccountController.cs        # Cookie-based auth
    │   └── AuthController.cs           # JWT API endpoints
    ├── Services/
    │   ├── IAuthenticationService.cs   # Service interface
    │   └── AuthenticationService.cs    # Service implementation
    └── Pages/
        └── Auth/
            ├── Login.razor             # Login page
            └── Register.razor          # Registration page
```

---

## Components

### 1. ApplicationUser Model
**Location:** `BlazorCrudDemo.Data/Models/ApplicationUser.cs`

Extended version of `IdentityUser` with additional properties:

```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    // Navigation properties
    public ICollection<AuditLog> AuditLogs { get; set; }
    public ICollection<UserActivity> Activities { get; set; }
    public ICollection<LoginHistory> LoginHistory { get; set; }
}
```

### 2. Authentication DTOs
**Location:** `BlazorCrudDemo.Shared/DTOs/AuthDtos.cs`

#### LoginDto
```csharp
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
```

#### RegisterDto
```csharp
public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } = "User";
}
```

#### AuthResult
```csharp
public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Errors { get; set; }
    public ApplicationUserDto? User { get; set; }
}
```

### 3. Authentication Service
**Location:** `BlazorCrudDemo.Web/Services/AuthenticationService.cs`

Core service implementing all authentication logic:

#### Interface Methods
```csharp
public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task<AuthResult> RegisterAsync(RegisterDto registerDto);
    Task<AuthResult> LogoutAsync();
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ResetPasswordAsync(string email);
    Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<ApplicationUserDto> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<List<Claim>> GetUserClaimsAsync();
    
    event Action AuthenticationStateChanged;
}
```

### 4. Controllers

#### AccountController (Cookie Authentication)
**Location:** `BlazorCrudDemo.Web/Controllers/AccountController.cs`

Handles traditional form-based authentication for Blazor Server pages:
- `GET/POST /Account/Login` - Login page and handler
- `GET/POST /Account/ForgotPassword` - Password reset request
- `GET/POST /Account/ResetPassword` - Password reset confirmation

#### AuthController (JWT Authentication)
**Location:** `BlazorCrudDemo.Web/Controllers/AuthController.cs`

RESTful API endpoints for JWT-based authentication:
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/register` - Register new user
- `POST /api/auth/logout` - Logout (requires authentication)
- `POST /api/auth/refresh-token` - Refresh JWT token
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/test-jwt` - Test JWT authentication

---

## Authentication Flow

### 1. Registration Flow
```
User fills registration form
        ↓
POST /api/auth/register
        ↓
AuthenticationService.RegisterAsync()
        ↓
Validate input (email uniqueness, password strength)
        ↓
Create ApplicationUser
        ↓
UserManager.CreateAsync(user, password)
        ↓
Assign role (User/Admin)
        ↓
Log audit event (USER_CREATED)
        ↓
Return AuthResult with user data
```

### 2. Login Flow (JWT)
```
User submits credentials
        ↓
POST /api/auth/login
        ↓
AuthenticationService.LoginAsync()
        ↓
Find user by email
        ↓
Check if account is active
        ↓
Validate password (with lockout protection)
        ↓
Generate JWT claims (user ID, email, roles)
        ↓
Create JWT access token (1 hour expiry)
        ↓
Generate refresh token (random 32 bytes)
        ↓
Store refresh token in user's authentication tokens
        ↓
Update LastLoginDate
        ↓
Log successful login to LoginHistory
        ↓
Return AuthResult with tokens and user data
```

### 3. Token Refresh Flow
```
Client sends expired/expiring access token + refresh token
        ↓
POST /api/auth/refresh-token
        ↓
AuthenticationService.RefreshTokenAsync()
        ↓
Validate refresh token
        ↓
Get current user from context
        ↓
Generate new JWT access token
        ↓
Generate new refresh token
        ↓
Update stored refresh token
        ↓
Return new tokens
```

### 4. Logout Flow
```
User clicks logout
        ↓
POST /api/auth/logout (with JWT in header)
        ↓
AuthenticationService.LogoutAsync()
        ↓
Get current user
        ↓
Log logout event to LoginHistory
        ↓
Remove refresh token from storage
        ↓
SignInManager.SignOutAsync()
        ↓
Trigger AuthenticationStateChanged event
        ↓
Client clears tokens and redirects to login
```

---

## API Reference

### Authentication Endpoints

#### POST /api/auth/login
Login and receive JWT tokens.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "rememberMe": false
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "xMzI0NTY3ODkwMTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkw",
  "expiresAt": "2024-11-04T07:00:00Z",
  "user": {
    "id": "user-id-123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"],
    "isActive": true,
    "createdDate": "2024-01-01T00:00:00Z",
    "lastLoginDate": "2024-11-04T06:00:00Z"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Invalid email or password",
  "errors": ["Invalid email or password"]
}
```

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "User"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "id": "new-user-id",
    "email": "newuser@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "roles": ["User"],
    "isActive": true
  }
}
```

#### POST /api/auth/refresh-token
Refresh an expired access token.

**Request Body:**
```json
"xMzI0NTY3ODkwMTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkw"
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-refresh-token-here",
  "expiresAt": "2024-11-04T08:00:00Z"
}
```

#### GET /api/auth/me
Get current authenticated user information.

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "user": {
    "id": "user-id-123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"],
    "isActive": true
  }
}
```

#### POST /api/auth/logout
Logout and invalidate tokens.

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

## Security Features

### 1. Password Security
- **Minimum Requirements:** Enforced by ASP.NET Core Identity
  - At least 6 characters
  - Requires uppercase letter
  - Requires lowercase letter
  - Requires digit
  - Requires non-alphanumeric character
- **Hashing:** PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey
- **Storage:** Only hashed passwords stored in database

### 2. Account Lockout
- **Enabled by default** for failed login attempts
- **Lockout Duration:** Configurable (default: 5 minutes)
- **Max Failed Attempts:** Configurable (default: 5)
- **Logged:** All lockout events recorded in LoginHistory

### 3. JWT Token Security
- **Algorithm:** HMAC-SHA256
- **Key Length:** 256+ bits (configured in appsettings.json)
- **Token Expiry:** 1 hour (access token)
- **Refresh Token:** Cryptographically random 32-byte string
- **Claims Included:**
  - `sub` - User ID
  - `email` - User email
  - `jti` - Unique token ID
  - `name` - Full name
  - `role` - User roles

### 4. Cookie Security
- **HttpOnly:** Enabled (prevents XSS attacks)
- **Secure:** SameAsRequest (HTTPS in production)
- **SameSite:** Lax (CSRF protection)
- **Sliding Expiration:** Enabled
- **Expiry:** 2 hours

### 5. Authorization Policies
Configured in `Program.cs`:

```csharp
// Admin-only access
options.AddPolicy("AdminOnly", policy => 
    policy.RequireRole("Admin"));

// User or Admin access
options.AddPolicy("UserOnly", policy => 
    policy.RequireRole("User", "Admin"));

// Product management (Admin + specific claim)
options.AddPolicy("CanManageProducts", policy =>
    policy.RequireRole("Admin")
          .RequireClaim("permission", "products.manage"));
```

### 6. Audit Logging
All authentication events are logged:
- Login attempts (success/failure)
- Logout events
- User registration
- Password changes
- Account lockouts
- IP address and user agent tracking

### 7. Active Account Validation
- Users can be deactivated (`IsActive = false`)
- Inactive users cannot log in
- Checked on every authentication attempt

---

## Usage Examples

### Example 1: Login from JavaScript
```javascript
async function login(email, password) {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            email: email,
            password: password,
            rememberMe: false
        })
    });
    
    const result = await response.json();
    
    if (result.success) {
        // Store tokens
        localStorage.setItem('accessToken', result.accessToken);
        localStorage.setItem('refreshToken', result.refreshToken);
        
        console.log('Logged in as:', result.user.email);
        return result.user;
    } else {
        console.error('Login failed:', result.message);
        throw new Error(result.message);
    }
}
```

### Example 2: Making Authenticated API Calls
```javascript
async function fetchProtectedData() {
    const token = localStorage.getItem('accessToken');
    
    const response = await fetch('/api/protected-endpoint', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    
    if (response.status === 401) {
        // Token expired, try to refresh
        await refreshToken();
        return fetchProtectedData(); // Retry
    }
    
    return await response.json();
}
```

### Example 3: Token Refresh
```javascript
async function refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    
    const response = await fetch('/api/auth/refresh-token', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(refreshToken)
    });
    
    const result = await response.json();
    
    if (result.success) {
        localStorage.setItem('accessToken', result.accessToken);
        localStorage.setItem('refreshToken', result.refreshToken);
    } else {
        // Refresh failed, redirect to login
        window.location.href = '/auth/login';
    }
}
```

### Example 4: Using in Blazor Components
```razor
@inject IAuthenticationService AuthService
@inject NavigationManager Navigation

<button @onclick="HandleLogin">Login</button>

@code {
    private async Task HandleLogin()
    {
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        };
        
        var result = await AuthService.LoginAsync(loginDto);
        
        if (result.Success)
        {
            Navigation.NavigateTo("/");
        }
        else
        {
            // Show error message
            Console.WriteLine(result.Message);
        }
    }
}
```

### Example 5: Protecting Blazor Pages
```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin")]

<h3>Admin Dashboard</h3>
<p>This page is only accessible to administrators.</p>
```

### Example 6: Protecting API Controllers
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
    [Authorize(Policy = "AdminOnly")] // Requires Admin role
    public async Task<IActionResult> CreateProduct()
    {
        // Implementation
    }
}
```

---

## Testing

### Manual Testing

#### 1. Test User Registration
```bash
curl -X POST https://localhost:5120/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "User"
  }'
```

#### 2. Test Login
```bash
curl -X POST https://localhost:5120/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

#### 3. Test Protected Endpoint
```bash
curl -X GET https://localhost:5120/api/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

#### 4. Test JWT Validation
```bash
curl -X GET https://localhost:5120/api/auth/test-jwt \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### Default Test Accounts

The application seeds default accounts on first run:

**Admin Account:**
- Email: `admin@blazorcrud.com`
- Password: `Admin123!`
- Role: Admin

**User Account:**
- Email: `user@blazorcrud.com`
- Password: `User123!`
- Role: User

### Testing Checklist

- [ ] User can register with valid credentials
- [ ] User cannot register with duplicate email
- [ ] User can login with correct credentials
- [ ] User cannot login with incorrect password
- [ ] Account locks after multiple failed attempts
- [ ] JWT token is generated on successful login
- [ ] JWT token contains correct claims
- [ ] Protected endpoints reject unauthenticated requests
- [ ] Protected endpoints accept valid JWT tokens
- [ ] Refresh token generates new access token
- [ ] Logout invalidates refresh token
- [ ] Inactive users cannot login
- [ ] Password reset generates token
- [ ] Login events are logged to database
- [ ] User roles are correctly assigned

---

## Troubleshooting

### Issue: "Invalid email or password" on correct credentials

**Possible Causes:**
1. User account is inactive (`IsActive = false`)
2. Account is locked out
3. Password was changed

**Solution:**
```sql
-- Check user status
SELECT Id, Email, IsActive, LockoutEnd FROM AspNetUsers WHERE Email = 'user@example.com';

-- Unlock account
UPDATE AspNetUsers SET LockoutEnd = NULL WHERE Email = 'user@example.com';

-- Activate account
UPDATE AspNetUsers SET IsActive = 1 WHERE Email = 'user@example.com';
```

### Issue: JWT token validation fails

**Possible Causes:**
1. Token expired
2. Invalid signature (wrong JWT secret key)
3. Issuer/Audience mismatch

**Solution:**
1. Check token expiry: Decode JWT at https://jwt.io
2. Verify `appsettings.json` JWT configuration matches
3. Ensure JWT key is at least 32 characters
4. Check server logs for specific validation errors

### Issue: "401 Unauthorized" on protected endpoints

**Possible Causes:**
1. Missing Authorization header
2. Malformed Bearer token
3. Token expired
4. User doesn't have required role/policy

**Solution:**
```javascript
// Correct format
headers: {
    'Authorization': 'Bearer ' + token  // Note the space after 'Bearer'
}

// Check token expiry
const payload = JSON.parse(atob(token.split('.')[1]));
const expiry = new Date(payload.exp * 1000);
console.log('Token expires:', expiry);
```

### Issue: Refresh token doesn't work

**Possible Causes:**
1. Refresh token not stored in database
2. User logged out (refresh token removed)
3. Refresh token format incorrect

**Solution:**
```sql
-- Check stored refresh tokens
SELECT UserId, LoginProvider, Name, Value 
FROM AspNetUserTokens 
WHERE Name = 'RefreshToken';
```

### Issue: CORS errors when calling API

**Solution:**
Ensure CORS is configured in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5120")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Issue: Database migration errors

**Solution:**
```bash
# Reset database
dotnet ef database drop --project BlazorCrudDemo.Data --startup-project BlazorCrudDemo.Web
dotnet ef database update --project BlazorCrudDemo.Data --startup-project BlazorCrudDemo.Web
```

---

## Configuration Reference

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/blazorcrud.db"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "https://localhost:5120",
    "Audience": "https://localhost:5120"
  }
}
```

### Identity Options (Program.cs)

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
});
```

---

## Best Practices

### 1. Token Storage
- **Web Apps:** Store in httpOnly cookies (most secure)
- **SPAs:** Store in memory or sessionStorage (not localStorage for sensitive apps)
- **Mobile Apps:** Use secure storage (Keychain/Keystore)

### 2. Token Expiry
- **Access Token:** Short-lived (15 min - 1 hour)
- **Refresh Token:** Long-lived (7-30 days)
- Always implement token refresh logic

### 3. Password Security
- Never log passwords
- Use HTTPS in production
- Implement rate limiting on login endpoint
- Consider adding CAPTCHA for repeated failures

### 4. Error Messages
- Don't reveal whether email exists ("Invalid email or password")
- Log detailed errors server-side
- Return generic errors to client

### 5. Audit Logging
- Log all authentication events
- Include IP address and user agent
- Monitor for suspicious patterns
- Implement alerting for anomalies

---

## Additional Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

**Last Updated:** November 4, 2024  
**Version:** 1.0.0
