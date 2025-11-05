# Authentication System - Testing Guide

## Table of Contents
1. [Quick Start Testing](#quick-start-testing)
2. [Manual Testing Scenarios](#manual-testing-scenarios)
3. [API Testing with cURL](#api-testing-with-curl)
4. [API Testing with Postman](#api-testing-with-postman)
5. [Automated Testing](#automated-testing)
6. [Security Testing](#security-testing)
7. [Performance Testing](#performance-testing)

---

## Quick Start Testing

### Prerequisites
1. Application is running on `https://localhost:5120`
2. Database is initialized with seed data
3. You have a REST client (cURL, Postman, or browser)

### Default Test Accounts

The application creates these accounts automatically on first run:

**Admin Account**
- Email: `admin@blazorcrud.com`
- Password: `Admin123!`
- Role: Admin
- Access: Full system access

**Regular User Account**
- Email: `user@blazorcrud.com`
- Password: `User123!`
- Role: User
- Access: Limited to user features

---

## Manual Testing Scenarios

### Scenario 1: User Registration

**Steps:**
1. Navigate to `https://localhost:5120/auth/register`
2. Fill in the registration form:
   - Email: `testuser@example.com`
   - Password: `Test123!`
   - Confirm Password: `Test123!`
   - First Name: `Test`
   - Last Name: `User`
   - Role: `User`
3. Click "Register"

**Expected Results:**
- ✅ Success message displayed
- ✅ User redirected to home page
- ✅ User is automatically logged in
- ✅ User record created in database
- ✅ Audit log entry created for USER_CREATED

**Validation Tests:**
- ❌ Registration with existing email should fail
- ❌ Password mismatch should show error
- ❌ Weak password should show validation error
- ❌ Invalid email format should show error

### Scenario 2: User Login (Web Form)

**Steps:**
1. Navigate to `https://localhost:5120/auth/login`
2. Enter credentials:
   - Email: `user@blazorcrud.com`
   - Password: `User123!`
3. Click "Log in"

**Expected Results:**
- ✅ Success message or redirect to home
- ✅ User's name appears in top navigation
- ✅ Login history record created
- ✅ LastLoginDate updated in user record
- ✅ Authentication cookie set

**Negative Tests:**
- ❌ Wrong password: "Invalid email or password"
- ❌ Non-existent email: "Invalid email or password"
- ❌ Inactive account: "Account is inactive"
- ❌ 5 failed attempts: Account locked for 30 minutes

### Scenario 3: JWT Token Authentication (API)

**Steps:**
1. Send POST request to `/api/auth/login`
2. Include credentials in JSON body
3. Receive JWT tokens in response
4. Use access token for subsequent API calls

**Expected Results:**
- ✅ Receive `accessToken` (valid for 1 hour)
- ✅ Receive `refreshToken` (for token renewal)
- ✅ Receive user object with roles
- ✅ Token contains correct claims
- ✅ Can access protected endpoints with token

### Scenario 4: Token Refresh

**Steps:**
1. Wait for access token to expire (or use an expired token)
2. Send POST request to `/api/auth/refresh-token`
3. Include refresh token in request body
4. Receive new tokens

**Expected Results:**
- ✅ New access token generated
- ✅ New refresh token generated
- ✅ Old refresh token invalidated
- ✅ Can use new token immediately

### Scenario 5: Logout

**Steps:**
1. While logged in, click "Logout" button
2. Observe behavior

**Expected Results:**
- ✅ User redirected to login page
- ✅ Authentication cookie cleared
- ✅ Refresh token removed from database
- ✅ Logout event logged
- ✅ Cannot access protected pages

### Scenario 6: Password Reset

**Steps:**
1. Navigate to `/Account/ForgotPassword`
2. Enter email address
3. Submit form

**Expected Results:**
- ✅ Password reset token generated
- ✅ Token logged to console (in development)
- ✅ Confirmation page displayed
- ✅ In production: Email sent with reset link

### Scenario 7: Role-Based Authorization

**Test Admin Access:**
1. Login as admin (`admin@blazorcrud.com`)
2. Navigate to `/admin/users`
3. Verify access granted

**Test User Access:**
1. Login as user (`user@blazorcrud.com`)
2. Try to navigate to `/admin/users`
3. Verify access denied or redirect

**Expected Results:**
- ✅ Admin can access admin pages
- ❌ Regular user cannot access admin pages
- ✅ Appropriate error message shown

### Scenario 8: Account Lockout

**Steps:**
1. Attempt login with wrong password 5 times
2. Try to login with correct password

**Expected Results:**
- ✅ After 5 failures: "Account is locked out"
- ✅ Lockout lasts 30 minutes
- ✅ All attempts logged in LoginHistory
- ❌ Cannot login even with correct password during lockout

---

## API Testing with cURL

### 1. Register New User

```bash
curl -X POST https://localhost:5120/api/auth/register \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "newuser@example.com",
    "password": "NewUser123!",
    "confirmPassword": "NewUser123!",
    "firstName": "New",
    "lastName": "User",
    "role": "User"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "id": "...",
    "email": "newuser@example.com",
    "firstName": "New",
    "lastName": "User",
    "roles": ["User"],
    "isActive": true
  }
}
```

### 2. Login and Get JWT Token

```bash
curl -X POST https://localhost:5120/api/auth/login \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "admin@blazorcrud.com",
    "password": "Admin123!"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "xMzI0NTY3ODkwMTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkw",
  "expiresAt": "2024-11-04T07:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@blazorcrud.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"]
  }
}
```

### 3. Access Protected Endpoint

```bash
# Save the token from previous response
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET https://localhost:5120/api/auth/me \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "user": {
    "id": "...",
    "email": "admin@blazorcrud.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"],
    "isActive": true
  }
}
```

### 4. Test JWT Validation

```bash
curl -X GET https://localhost:5120/api/auth/test-jwt \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "message": "JWT authentication is working!",
  "timestamp": "2024-11-04T06:00:00Z"
}
```

### 5. Refresh Token

```bash
REFRESH_TOKEN="xMzI0NTY3ODkwMTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkw"

curl -X POST https://localhost:5120/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -k \
  -d "\"$REFRESH_TOKEN\""
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-refresh-token-here",
  "expiresAt": "2024-11-04T08:00:00Z"
}
```

### 6. Logout

```bash
curl -X POST https://localhost:5120/api/auth/logout \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

## API Testing with Postman

### Setup Postman Collection

1. **Create New Collection:** "BlazorCrudDemo Auth"
2. **Add Collection Variables:**
   - `baseUrl`: `https://localhost:5120`
   - `accessToken`: (will be set automatically)
   - `refreshToken`: (will be set automatically)

### Request 1: Register

- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/register`
- **Headers:** `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "email": "postman@example.com",
  "password": "Postman123!",
  "confirmPassword": "Postman123!",
  "firstName": "Postman",
  "lastName": "Tester",
  "role": "User"
}
```

### Request 2: Login

- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/login`
- **Headers:** `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "email": "admin@blazorcrud.com",
  "password": "Admin123!"
}
```
- **Tests Script:**
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.collectionVariables.set("accessToken", response.accessToken);
    pm.collectionVariables.set("refreshToken", response.refreshToken);
}
```

### Request 3: Get Current User

- **Method:** GET
- **URL:** `{{baseUrl}}/api/auth/me`
- **Headers:** 
  - `Authorization: Bearer {{accessToken}}`

### Request 4: Test JWT

- **Method:** GET
- **URL:** `{{baseUrl}}/api/auth/test-jwt`
- **Headers:** 
  - `Authorization: Bearer {{accessToken}}`

### Request 5: Refresh Token

- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/refresh-token`
- **Headers:** `Content-Type: application/json`
- **Body (raw):**
```
"{{refreshToken}}"
```
- **Tests Script:**
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.collectionVariables.set("accessToken", response.accessToken);
    pm.collectionVariables.set("refreshToken", response.refreshToken);
}
```

### Request 6: Logout

- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/logout`
- **Headers:** 
  - `Authorization: Bearer {{accessToken}}`

---

## Automated Testing

### Unit Test Example (xUnit)

```csharp
using Xunit;
using Moq;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using BlazorCrudDemo.Data.Models;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var mockUserManager = MockUserManager<ApplicationUser>();
        var mockSignInManager = MockSignInManager<ApplicationUser>();
        // ... setup mocks
        
        var service = new AuthenticationService(
            mockUserManager.Object,
            mockSignInManager.Object,
            // ... other dependencies
        );
        
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };
        
        // Act
        var result = await service.LoginAsync(loginDto);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }
    
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        // ... setup
        
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };
        
        // Act
        var result = await service.LoginAsync(loginDto);
        
        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid email or password", result.Message);
    }
}
```

### Integration Test Example

```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@blazorcrud.com",
            Password = "Admin123!"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(result?.AccessToken);
    }
}
```

---

## Security Testing

### 1. SQL Injection Testing

**Test:** Try SQL injection in login form
```bash
curl -X POST https://localhost:5120/api/auth/login \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "admin@blazorcrud.com'\'' OR '\''1'\''='\''1",
    "password": "anything"
  }'
```

**Expected:** Request should fail safely without exposing database structure

### 2. JWT Token Tampering

**Test:** Modify JWT token payload
1. Get valid JWT token
2. Decode at https://jwt.io
3. Change user ID or role
4. Re-encode with wrong signature
5. Try to use modified token

**Expected:** Token validation should fail with 401 Unauthorized

### 3. Brute Force Protection

**Test:** Automated login attempts
```bash
for i in {1..10}; do
  curl -X POST https://localhost:5120/api/auth/login \
    -H "Content-Type: application/json" \
    -k \
    -d '{
      "email": "admin@blazorcrud.com",
      "password": "WrongPassword'$i'"
    }'
  sleep 1
done
```

**Expected:** Account locked after 5 attempts

### 4. Token Expiry Testing

**Test:** Use expired token
1. Get valid token
2. Wait for expiry (or manually set system time forward)
3. Try to access protected endpoint

**Expected:** 401 Unauthorized with "Token-Expired" header

### 5. CSRF Protection

**Test:** Cross-site request forgery
- Try to submit login form from different origin
- Check for anti-forgery tokens

**Expected:** Request blocked or token validation required

---

## Performance Testing

### Load Test with Apache Bench

```bash
# Test login endpoint
ab -n 1000 -c 10 -p login.json -T application/json \
  https://localhost:5120/api/auth/login
```

**login.json:**
```json
{
  "email": "user@blazorcrud.com",
  "password": "User123!"
}
```

**Expected Metrics:**
- Requests per second: > 100
- Mean response time: < 100ms
- Failed requests: 0

### Concurrent User Testing

```bash
# Simulate 50 concurrent users
for i in {1..50}; do
  (
    TOKEN=$(curl -s -X POST https://localhost:5120/api/auth/login \
      -H "Content-Type: application/json" \
      -k \
      -d '{"email":"user@blazorcrud.com","password":"User123!"}' \
      | jq -r '.accessToken')
    
    curl -s -X GET https://localhost:5120/api/auth/me \
      -H "Authorization: Bearer $TOKEN" \
      -k
  ) &
done
wait
```

---

## Database Verification

### Check User Creation

```sql
SELECT Id, Email, FirstName, LastName, IsActive, CreatedDate 
FROM AspNetUsers 
WHERE Email = 'testuser@example.com';
```

### Check Login History

```sql
SELECT u.Email, lh.LoginTime, lh.IsSuccessful, lh.IpAddress, lh.FailureReason
FROM LoginHistory lh
JOIN AspNetUsers u ON lh.UserId = u.Id
ORDER BY lh.LoginTime DESC
LIMIT 10;
```

### Check Audit Logs

```sql
SELECT u.Email, al.Action, al.EntityName, al.Timestamp, al.Changes
FROM AuditLogs al
JOIN AspNetUsers u ON al.UserId = u.Id
WHERE al.Action LIKE '%AUTH%'
ORDER BY al.Timestamp DESC
LIMIT 10;
```

### Check Refresh Tokens

```sql
SELECT u.Email, ut.LoginProvider, ut.Name, LENGTH(ut.Value) as TokenLength
FROM AspNetUserTokens ut
JOIN AspNetUsers u ON ut.UserId = u.Id
WHERE ut.Name = 'RefreshToken';
```

---

## Testing Checklist

### Functional Tests
- [ ] User registration with valid data succeeds
- [ ] User registration with duplicate email fails
- [ ] Login with valid credentials succeeds
- [ ] Login with invalid credentials fails
- [ ] JWT token is generated on successful login
- [ ] JWT token contains correct claims
- [ ] Protected endpoints require authentication
- [ ] Protected endpoints accept valid tokens
- [ ] Token refresh works correctly
- [ ] Logout clears authentication state
- [ ] Password reset generates token
- [ ] Role-based authorization works

### Security Tests
- [ ] Passwords are hashed in database
- [ ] SQL injection attempts fail safely
- [ ] JWT tampering is detected
- [ ] Account lockout after failed attempts
- [ ] CSRF protection is enabled
- [ ] HTTPS is enforced in production
- [ ] Tokens expire correctly
- [ ] Refresh tokens are invalidated on logout

### Performance Tests
- [ ] Login handles 100+ requests/second
- [ ] Token validation is fast (< 10ms)
- [ ] Database queries are optimized
- [ ] No memory leaks in long-running sessions

### Audit Tests
- [ ] All login attempts are logged
- [ ] User creation is logged
- [ ] Password changes are logged
- [ ] Logout events are logged
- [ ] IP addresses are captured
- [ ] User agents are captured

---

## Troubleshooting Test Failures

### "Connection refused" errors
- Ensure application is running
- Check correct port (5120)
- Verify HTTPS certificate

### "401 Unauthorized" on valid token
- Check token hasn't expired
- Verify JWT secret key matches
- Check Issuer/Audience configuration

### Database errors
- Run migrations: `dotnet ef database update`
- Check connection string
- Verify database file permissions

### Lockout not working
- Check Identity configuration in Program.cs
- Verify lockout settings
- Check database LoginHistory table

---

**Last Updated:** November 4, 2024  
**Version:** 1.0.0
