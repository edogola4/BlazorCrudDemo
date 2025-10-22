using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace BlazorCrudDemo.Web.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ApplicationUserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u => u.LoginHistory)
                    .ToListAsync();

                var userDtos = new List<ApplicationUserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new ApplicationUserDto
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
                    });
                }

                return userDtos.OrderBy(u => u.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<ApplicationUserDto>();
            }
        }

        public async Task<ApplicationUserDto> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.Users
                    .Include(u => u.LoginHistory)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return null;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                return null;
            }
        }

        public async Task<AuthResult> CreateUserAsync(RegisterDto registerDto, string createdBy)
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

                // Validate role exists
                if (!await _roleManager.RoleExistsAsync(registerDto.Role))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid role specified",
                        Errors = new List<string> { "Invalid role" }
                    };
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
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
                    createdBy,
                    "USER_CREATED",
                    $"User account created for {registerDto.Email}",
                    $"Role: {registerDto.Role}, Created by: {createdBy}",
                    null,
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "User created successfully",
                    User = await MapToUserDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", registerDto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while creating user",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> UpdateUserAsync(string userId, UpdateUserDto updateDto, string modifiedBy)
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

                // Check if email is being changed and if it's already taken
                if (updateDto.Email != null && updateDto.Email != user.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                    if (existingUser != null)
                    {
                        return new AuthResult
                        {
                            Success = false,
                            Message = "Email is already in use",
                            Errors = new List<string> { "Email already in use" }
                        };
                    }
                    user.Email = updateDto.Email;
                    user.UserName = updateDto.Email;
                    user.NormalizedEmail = updateDto.Email.ToUpper();
                    user.NormalizedUserName = updateDto.Email.ToUpper();
                }

                if (updateDto.FirstName != null)
                    user.FirstName = updateDto.FirstName;

                if (updateDto.LastName != null)
                    user.LastName = updateDto.LastName;

                if (updateDto.ProfileImageUrl != null)
                    user.ProfileImageUrl = updateDto.ProfileImageUrl;

                user.IsActive = updateDto.IsActive;
                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = modifiedBy;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to update user",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _auditService.LogUserActivityAsync(
                    modifiedBy,
                    "USER_UPDATED",
                    $"User {user.Email} updated",
                    $"Updated by: {modifiedBy}",
                    Convert.ToInt32(userId),
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "User updated successfully",
                    User = await MapToUserDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while updating user",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> DeleteUserAsync(string userId, string deletedBy)
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

                // Soft delete by deactivating the user
                user.IsActive = false;
                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = deletedBy;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to delete user",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _auditService.LogUserActivityAsync(
                    deletedBy,
                    "USER_DELETED",
                    $"User {user.Email} deactivated",
                    $"Deleted by: {deletedBy}",
                    Convert.ToInt32(userId),
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "User deactivated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while deleting user",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> AssignRoleAsync(string userId, string roleName, string assignedBy)
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

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Role does not exist",
                        Errors = new List<string> { "Role does not exist" }
                    };
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to assign role",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _auditService.LogUserActivityAsync(
                    assignedBy,
                    "ROLE_ASSIGNED",
                    $"Role {roleName} assigned to {user.Email}",
                    $"Assigned by: {assignedBy}",
                    Convert.ToInt32(userId),
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "Role assigned successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while assigning role",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<AuthResult> RemoveRoleAsync(string userId, string roleName, string removedBy)
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

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to remove role",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _auditService.LogUserActivityAsync(
                    removedBy,
                    "ROLE_REMOVED",
                    $"Role {roleName} removed from {user.Email}",
                    $"Removed by: {removedBy}",
                    Convert.ToInt32(userId),
                    "User",
                    GetClientIpAddress(),
                    GetUserAgent());

                return new AuthResult
                {
                    Success = true,
                    Message = "Role removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while removing role",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                var roles = await _userManager.GetRolesAsync(user);
                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is in role {RoleName}", userId, roleName);
                return false;
            }
        }

        public async Task<List<LoginHistoryDto>> GetUserLoginHistoryAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var loginHistory = await _userManager.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.LoginHistory)
                    .OrderByDescending(l => l.LoginTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return loginHistory.Select(l => new LoginHistoryDto
                {
                    Id = l.Id,
                    LoginTime = l.LoginTime,
                    LogoutTime = l.LogoutTime,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    IsSuccessful = l.IsSuccessful,
                    FailureReason = l.FailureReason,
                    SessionDuration = l.SessionDuration
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login history for user {UserId}", userId);
                return new List<LoginHistoryDto>();
            }
        }

        public async Task<List<UserActivityDto>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var activities = await _userManager.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Activities)
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return activities.Select(a => new UserActivityDto
                {
                    Id = a.Id,
                    ActivityType = a.ActivityType,
                    Description = a.Description,
                    Details = a.Details,
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    EntityId = a.EntityId,
                    EntityType = a.EntityType
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
                return new List<UserActivityDto>();
            }
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
