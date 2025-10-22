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

namespace BlazorCrudDemo.Web.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    await _auditService.LogLoginAsync("unknown", GetClientIpAddress(), GetUserAgent(), false, "User not found");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "Invalid email or password" }
                    };
                }

                if (!user.IsActive)
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Account is inactive");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is inactive. Please contact administrator.",
                        Errors = new List<string> { "Account is inactive" }
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), true);

                    var token = await GenerateJwtTokenAsync(user);
                    var refreshToken = GenerateRefreshToken();

                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Store refresh token
                    await _userManager.SetAuthenticationTokenAsync(user, "BlazorCrudDemo", "RefreshToken", refreshToken);

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Login successful",
                        AccessToken = token,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddHours(1),
                        User = await MapToUserDtoAsync(user)
                    };
                }

                if (result.IsLockedOut)
                {
                    await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Account locked out");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is locked out due to too many failed attempts",
                        Errors = new List<string> { "Account locked out" }
                    };
                }

                await _auditService.LogLoginAsync(user.Id, GetClientIpAddress(), GetUserAgent(), false, "Invalid password");
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Invalid email or password" }
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

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign role {Role} to user {Email}", registerDto.Role, registerDto.Email);
                }

                await _auditService.LogUserActivityAsync(
                    user.Id,
                    "USER_CREATED",
                    $"User account created for {registerDto.Email}",
                    $"Role: {registerDto.Role}",
                    null,
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

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
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
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
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
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
                issuer: _configuration["Jwt:Issuer"] ?? "https://localhost:5001",
                audience: _configuration["Jwt:Audience"] ?? "https://localhost:5001",
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

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

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";
        }
    }
}
