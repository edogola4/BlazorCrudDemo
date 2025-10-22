using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Data.Models;

namespace BlazorCrudDemo.Web.Services
{
    public interface IUserService
    {
        Task<List<ApplicationUserDto>> GetAllUsersAsync();
        Task<ApplicationUserDto> GetUserByIdAsync(string userId);
        Task<AuthResult> CreateUserAsync(RegisterDto registerDto, string createdBy);
        Task<AuthResult> UpdateUserAsync(string userId, UpdateUserDto updateDto, string modifiedBy);
        Task<AuthResult> DeleteUserAsync(string userId, string deletedBy);
        Task<AuthResult> AssignRoleAsync(string userId, string roleName, string assignedBy);
        Task<AuthResult> RemoveRoleAsync(string userId, string roleName, string removedBy);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<List<LoginHistoryDto>> GetUserLoginHistoryAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<UserActivityDto>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 20);
    }
}
