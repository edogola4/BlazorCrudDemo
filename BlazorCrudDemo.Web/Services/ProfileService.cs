using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BlazorCrudDemo.Data.Models;

namespace BlazorCrudDemo.Web.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);

            if (user == null)
            {
                return;
            }

            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();

            // Add custom claims
            claims.Add(new Claim("name", $"{user.FirstName} {user.LastName}".Trim()));
            claims.Add(new Claim("given_name", user.FirstName ?? ""));
            claims.Add(new Claim("family_name", user.LastName ?? ""));
            claims.Add(new Claim("profile_image", user.ProfileImageUrl ?? ""));

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            // Add permissions based on roles
            if (roles.Contains("Admin"))
            {
                claims.Add(new Claim("permission", "products.manage"));
                claims.Add(new Claim("permission", "categories.manage"));
                claims.Add(new Claim("permission", "users.manage"));
                claims.Add(new Claim("permission", "audit.view"));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);

            context.IsActive = user != null && user.IsActive;
        }
    }
}
