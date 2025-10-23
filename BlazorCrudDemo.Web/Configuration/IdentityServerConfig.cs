using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace BlazorCrudDemo.Web.Configuration
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = "roles",
                    DisplayName = "Roles",
                    Description = "User roles",
                    UserClaims = new List<string> { "role" }
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("blazorcrud.api", "Blazor CRUD API"),
                new ApiScope("products.api", "Products API"),
                new ApiScope("categories.api", "Categories API"),
                new ApiScope("users.api", "Users API")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "blazor-crud-client",
                    ClientName = "Blazor CRUD Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,
                    AllowOfflineAccess = true,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:5120/authentication/login-callback",
                        "https://localhost:5120/authentication/logout-callback"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:5120/"
                    },

                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "email",
                        "roles",
                        "blazorcrud.api",
                        "products.api",
                        "categories.api",
                        "users.api"
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "https://localhost:5120"
                    },

                    AccessTokenLifetime = 3600, // 1 hour
                    IdentityTokenLifetime = 3600, // 1 hour
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    AbsoluteRefreshTokenLifetime = 2592000 // 30 days
                },

                // JavaScript Client (for API calls)
                new Client
                {
                    ClientId = "blazor-crud-js",
                    ClientName = "Blazor CRUD JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:5120/authentication/login-callback"
                    },

                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "email",
                        "roles",
                        "blazorcrud.api"
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "https://localhost:5120"
                    },

                    AccessTokenLifetime = 3600, // 1 hour
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    AbsoluteRefreshTokenLifetime = 2592000 // 30 days
                }
            };
        }
    }
}
