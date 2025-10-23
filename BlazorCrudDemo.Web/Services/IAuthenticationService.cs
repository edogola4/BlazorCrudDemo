using BlazorCrudDemo.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlazorCrudDemo.Web.Services
{
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
        
        // Event for authentication state changes
        event Action AuthenticationStateChanged;
    }
}
