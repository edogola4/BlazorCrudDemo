using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorCrudDemo.Web.Services
{
    /// <summary>
    /// Implementation of authentication service that handles user authentication,
    /// JWT token generation, session management, and audit logging.
    /// Integrates with ASP.NET Core Identity for user management.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
        /// <summary>
        /// Event that fires when authentication state changes (login/logout).
        /// Components can subscribe to this to update UI when auth state changes.
        /// </summary>
        public event Action? AuthenticationStateChanged;

        /// <summary>
        /// Extracts the User-Agent header from the current HTTP request.
        /// Used for audit logging to track which browsers/devices are accessing the system.
        /// </summary>
        /// <returns>User-Agent string or "Unknown" if not available</returns>
        private string GetUserAgent()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            IAuditService auditService,
            AuthenticationStateProvider authenticationStateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    // Don't log to LoginHistory for non-existent users to avoid FK constraint violations
                    _logger.LogWarning("Login attempt for non-existent user: {Email}", loginDto.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "Invalid email or password" }
                    };
                }

                if (!user.IsActive)
                {
                    // Only log if the user exists and is inactive
                    if (await _userManager.FindByIdAsync(user.Id) != null)
                    {
                        await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Account is inactive");
                    }
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is inactive. Please contact administrator.",
                        Errors = new List<string> { "Account is inactive" }
                    };
                }

                // Verify password without signing in (to avoid cookie issues in Blazor Server)
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                
                if (!isPasswordValid)
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Sign in failed - Invalid password");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid login attempt",
                        Errors = new List<string> { "Invalid username or password" }
                    };
                }
                
                // Check if account is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Account locked out");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is locked out due to too many failed attempts",
                        Errors = new List<string> { "Account locked out" }
                    };
                }

                // Get user roles and add them as claims
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Add role claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), true, "Login successful");

                var token = await GenerateJwtTokenAsync(user);
                var refreshToken = GenerateRefreshToken();

                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Store refresh token
                await _userManager.SetAuthenticationTokenAsync(user, "BlazorCrudDemo", "RefreshToken", refreshToken);
                
                // Sign in the user with the authentication scheme
                await _signInManager.SignInWithClaimsAsync(user, loginDto.RememberMe, new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                });
                
                // Notify authentication state change
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
                {
                    customAuthProvider.MarkUserAsAuthenticated(principal);
                }
                
                NotifyAuthenticationStateChanged();

                // Check if user is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Account locked out");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is locked out due to too many failed attempts",
                        Errors = new List<string> { "Account locked out" }
                    };
                }

                // Notify subscribers of authentication state change
                NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = await MapToUserDtoAsync(user),
                    RequiresSignIn = true // Flag to indicate sign-in needs to be completed separately
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginDto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Errors = new List<string> { "Email already registered" }
                    };
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // First create the user
                var createResult = await _userManager.CreateAsync(user, registerDto.Password);
                if (!createResult.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = createResult.Errors.Select(e => e.Description).ToList()
                    };
                }

                try
                {
                    // Assign role
                    var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign role {Role} to user {Email}", registerDto.Role, registerDto.Email);
                    }

                    // Now that user is created, we can safely log the activity
                    await _auditService.LogUserActivityAsync(
                        user.Id,
                        "USER_CREATED",
                        $"User account created for {registerDto.Email}",
                        $"Role: {registerDto.Role}",
                        null,
                        "User",
                        GetClientIpAddress(),
                        GetUserAgent());
                }
                catch (Exception ex)
                {
                    // If anything fails after user creation, log the error but don't fail the registration
                    _logger.LogError(ex, "Error in post-user-creation steps for user {Email}", registerDto.Email);
                }

                return new AuthResult
                {
                    Success = true,
                    Message = "User registered successfully",
                    User = await MapToUserDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", registerDto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> LogoutAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    await _auditService.LogLogoutAsync(user.Id);

                    // Clear refresh token
                    await _userManager.RemoveAuthenticationTokenAsync(
                        await _userManager.FindByIdAsync(user.Id),
                        "BlazorCrudDemo",
                        "RefreshToken");
                }

                await _signInManager.SignOutAsync();

                // Notify subscribers of authentication state change
                NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    Message = "Logged out successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
                return false;
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // In a real implementation, you would validate the refresh token
                // For now, we'll generate a new token
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid refresh token",
                        Errors = new List<string> { "Invalid refresh token" }
                    };
                }

                var token = await GenerateJwtTokenAsync(await _userManager.FindByIdAsync(user.Id));
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token
                await _userManager.SetAuthenticationTokenAsync(
                    await _userManager.FindByIdAsync(user.Id),
                    "BlazorCrudDemo",
                    "RefreshToken",
                    newRefreshToken);

                // Notify subscribers of authentication state change
                NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while refreshing token",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return false;

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // In a real implementation, you would send this token via email
                _logger.LogInformation("Password reset token generated for user {Email}: {Token}", email, token);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for user {Email}", email);
                return false;
            }
        }

        public async Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = new List<string> { "User not found" }
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to change password",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _auditService.LogUserActivityAsync(
                    userId,
                    "PASSWORD_CHANGED",
                    "User changed password",
                    null,
                    null,
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while changing password",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<ApplicationUserDto> GetCurrentUserAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User == null)
                    return null;

                var user = await _userManager.GetUserAsync(httpContext.User);
                if (user == null)
                    return null;

                return await MapToUserDtoAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User == null)
                    return false;

                var user = await _userManager.GetUserAsync(httpContext.User);
                return user != null && user.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public async Task<List<Claim>> GetUserClaimsAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
                if (user == null)
                    return new List<Claim>();

                var claims = await _userManager.GetClaimsAsync(user);
                return claims.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user claims");
                return new List<Claim>();
            }
        }

        /// <summary>
        /// Generates a JWT access token for the specified user.
        /// Token includes user ID, email, name, and roles as claims.
        /// Token is signed with HMAC-SHA256 and expires in 1 hour.
        /// </summary>
        /// <param name="user">The user to generate a token for</param>
        /// <returns>JWT token string</returns>
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("first_name", user.FirstName ?? ""),
                new Claim("last_name", user.LastName ?? "")
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "https://localhost:5120",
                audience: _configuration["Jwt:Audience"] ?? "https://localhost:5120",
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// Refresh tokens are used to obtain new access tokens without re-authentication.
        /// </summary>
        /// <returns>Base64-encoded random token string</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Maps an ApplicationUser entity to an ApplicationUserDto for API responses.
        /// Includes user roles in the DTO.
        /// </summary>
        /// <param name="user">The user entity to map</param>
        /// <returns>User DTO with roles populated</returns>
        private async Task<ApplicationUserDto> MapToUserDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new ApplicationUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList()
            };
        }

        /// <summary>
        /// Retrieves the client's IP address from the current HTTP context.
        /// Used for audit logging and security tracking.
        /// </summary>
        /// <returns>IP address string or "unknown" if not available</returns>
        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Notifies all subscribers that the authentication state has changed.
        /// This triggers UI updates in components that depend on auth state.
        /// </summary>
        private void NotifyAuthenticationStateChanged()
        {
            AuthenticationStateChanged?.Invoke();
        }
    }
}
