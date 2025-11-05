using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using BlazorCrudDemo.Data.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace BlazorCrudDemo.Web.Services
{
    /// <summary>
    /// Custom authentication state provider for Blazor Server that integrates with ASP.NET Core Identity.
    /// This provider manages the authentication state and notifies Blazor components when the state changes.
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;
        private Task<AuthenticationState>? _authenticationStateTask;

        /// <summary>
        /// Initializes a new instance of the CustomAuthenticationStateProvider.
        /// </summary>
        /// <param name="userManager">ASP.NET Core Identity UserManager for user operations</param>
        /// <param name="signInManager">ASP.NET Core Identity SignInManager for authentication operations</param>
        /// <param name="httpContextAccessor">Accessor for the current HTTP context</param>
        /// <param name="logger">Logger for diagnostic information</param>
        public CustomAuthenticationStateProvider(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CustomAuthenticationStateProvider> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current authentication state asynchronously.
        /// This method is called by Blazor to determine if a user is authenticated and what claims they have.
        /// </summary>
        /// <returns>An AuthenticationState containing the current user's ClaimsPrincipal</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Always get a fresh state to ensure we have the latest authentication info
                var state = await GetAuthenticationStateInternalAsync();
                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private async Task<AuthenticationState> GetAuthenticationStateInternalAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                
                // If there's no HTTP context, return an anonymous user
                if (httpContext == null)
                {
                    _logger.LogDebug("No HTTP context available, returning anonymous user");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Get the user from the HTTP context
                var user = httpContext.User;

                // If the user is authenticated, verify they exist in the database and are active
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(user);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var applicationUser = await _userManager.FindByIdAsync(userId);
                        
                        // If user doesn't exist or is inactive, sign them out
                        if (applicationUser == null || !applicationUser.IsActive)
                        {
                            _logger.LogWarning("User {UserId} not found or inactive, signing out", userId);
                            await _signInManager.SignOutAsync();
                            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                        }

                        // User is valid and active, return their authenticated state
                        _logger.LogDebug("User {UserId} authenticated successfully", userId);
                        return new AuthenticationState(user);
                    }
                }

                // User is not authenticated
                _logger.LogDebug("User is not authenticated");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                // On error, return anonymous user to prevent application crashes
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        /// <summary>
        /// Marks a user as authenticated with the specified claims.
        /// This method should be called after a successful login.
        /// </summary>
        /// <param name="user">The authenticated user</param>
        public void MarkUserAsAuthenticated(ClaimsPrincipal user)
        {
            _logger.LogInformation("Marking user as authenticated");
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Marks the current user as logged out.
        /// This method should be called after a logout operation.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _logger.LogInformation("Marking user as logged out");
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Refreshes the authentication state by re-reading the current user's claims.
        /// Useful when user roles or claims have been updated.
        /// </summary>
        public async Task RefreshAuthenticationStateAsync()
        {
            _logger.LogInformation("Refreshing authentication state");
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
    }
}
