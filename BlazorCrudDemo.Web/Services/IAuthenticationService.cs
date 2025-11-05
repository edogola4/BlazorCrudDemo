using BlazorCrudDemo.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlazorCrudDemo.Web.Services
{
    /// <summary>
    /// Service interface for handling authentication operations including login, registration,
    /// token management, and user session management.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user with email and password credentials.
        /// </summary>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>AuthResult containing JWT tokens and user information if successful</returns>
        Task<AuthResult> LoginAsync(LoginDto loginDto);
        /// <summary>
        /// Registers a new user account in the system.
        /// </summary>
        /// <param name="registerDto">Registration data including email, password, and user details</param>
        /// <returns>AuthResult indicating success or failure with error messages</returns>
        Task<AuthResult> RegisterAsync(RegisterDto registerDto);
        /// <summary>
        /// Logs out the current user, invalidating their session and refresh token.
        /// </summary>
        /// <returns>AuthResult indicating logout success</returns>
        Task<AuthResult> LogoutAsync();
        /// <summary>
        /// Confirms a user's email address using a confirmation token.
        /// </summary>
        /// <param name="userId">The ID of the user confirming their email</param>
        /// <param name="token">Email confirmation token</param>
        /// <returns>True if confirmation successful, false otherwise</returns>
        Task<bool> ConfirmEmailAsync(string userId, string token);
        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to use for generating new tokens</param>
        /// <returns>AuthResult with new access and refresh tokens</returns>
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        /// <summary>
        /// Initiates a password reset process for a user.
        /// </summary>
        /// <param name="email">Email address of the user requesting password reset</param>
        /// <returns>True if reset token generated successfully</returns>
        Task<bool> ResetPasswordAsync(string email);
        /// <summary>
        /// Changes a user's password after validating their current password.
        /// </summary>
        /// <param name="userId">ID of the user changing their password</param>
        /// <param name="currentPassword">User's current password for verification</param>
        /// <param name="newPassword">New password to set</param>
        /// <returns>AuthResult indicating success or failure</returns>
        Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        /// <summary>
        /// Retrieves the currently authenticated user's information.
        /// </summary>
        /// <returns>ApplicationUserDto with user details, or null if not authenticated</returns>
        Task<ApplicationUserDto> GetCurrentUserAsync();
        /// <summary>
        /// Checks if the current user is authenticated and active.
        /// </summary>
        /// <returns>True if user is authenticated and active, false otherwise</returns>
        Task<bool> IsAuthenticatedAsync();
        /// <summary>
        /// Retrieves all claims associated with the current user.
        /// </summary>
        /// <returns>List of user claims</returns>
        Task<List<Claim>> GetUserClaimsAsync();
        
        /// <summary>
        /// Event that fires when the authentication state changes (login/logout).
        /// Subscribe to this event to be notified of authentication state changes.
        /// </summary>
        event Action AuthenticationStateChanged;
    }
}
