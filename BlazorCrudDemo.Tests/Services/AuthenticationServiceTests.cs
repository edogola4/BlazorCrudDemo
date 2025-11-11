using System;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Tests.TestHelpers;
using BlazorCrudDemo.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlazorCrudDemo.Tests.Services
{
    public class AuthenticationServiceTests : IClassFixture<TestFixture<Program>>, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthenticationService _authService;
        private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        public AuthenticationServiceTests(TestFixture<Program> factory)
        {
            // Create a new service provider and in-memory database for each test
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddLogging()
                .BuildServiceProvider();

            // Create a new options instance using an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthTestDb_" + Guid.NewGuid())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            // Create the ApplicationDbContext
            _context = new ApplicationDbContext(
                options,
                new AuditInterceptor(),
                new SoftDeleteInterceptor()
            );

            // Create UserManager and SignInManager
            var userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(
                userStore,
                null, // options
                new PasswordHasher<ApplicationUser>(),
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                serviceProvider,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );

            _signInManager = new SignInManager<ApplicationUser>(
                _userManager,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, // options
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                null, // authentication schemes
                null  // user confirmation
            );

            // Create mocks
            _loggerMock = new Mock<ILogger<AuthenticationService>>();
            _tokenServiceMock = new Mock<ITokenService>();

            // Setup token service mock
            _tokenServiceMock.Setup(t => t.GenerateJwtToken(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-jwt-token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
                .Returns(new RefreshToken
                {
                    Token = "test-refresh-token",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                });

            // Create the authentication service
            _authService = new AuthenticationService(
                _userManager,
                _signInManager,
                _tokenServiceMock.Object,
                _loggerMock.Object
            );

            // Seed the database with test data
            DatabaseInitializer.Initialize(_context);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@test.com",
                Password = "Admin123!",
                RememberMe = false
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().Be("test-jwt-token");
            result.RefreshToken.Should().Be("test-refresh-token");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@test.com",
                Password = "WrongPassword123!",
                RememberMe = false
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().NotBeEmpty();
            result.Token.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "newuser@test.com",
                Password = "NewUser123!",
                ConfirmPassword = "NewUser123!"
            };

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Registration successful");

            // Verify the user was created in the database
            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            user.Should().NotBeNull();
            user.FirstName.Should().Be(registerDto.FirstName);
            user.LastName.Should().Be(registerDto.LastName);
        }

        [Fact]
        public async Task LogoutAsync_ShouldSignOutUser()
        {
            // Act
            var result = await _authService.LogoutAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logout successful");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCurrentPassword_ShouldUpdatePassword()
        {
            // Arrange
            var user = await _userManager.FindByEmailAsync("admin@test.com");
            var currentPassword = "Admin123!";
            var newPassword = "NewPassword123!";

            // Act
            var result = await _authService.ChangePasswordAsync(user.Id, currentPassword, newPassword);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password changed successfully");

            // Verify the password was actually changed
            var passwordCheck = await _userManager.CheckPasswordAsync(user, newPassword);
            passwordCheck.Should().BeTrue();
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserIsAuthenticated_ShouldReturnUser()
        {
            // Arrange
            var user = await _userManager.FindByEmailAsync("admin@test.com");
            
            // Mock the current user
            var claims = await _userManager.GetClaimsAsync(user);
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
            
            // This would normally be set by the authentication middleware
            _authService.SetCurrentUser(claimsPrincipal);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(user.Email);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        [Fact]
        public async Task IsAuthenticatedAsync_WhenUserIsAuthenticated_ShouldReturnTrue()
        {
            // Arrange
            var user = await _userManager.FindByEmailAsync("admin@test.com");
            var claims = await _userManager.GetClaimsAsync(user);
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
            _authService.SetCurrentUser(claimsPrincipal);

            // Act
            var result = await _authService.IsAuthenticatedAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserClaimsAsync_WhenUserIsAuthenticated_ShouldReturnUserClaims()
        {
            // Arrange
            var user = await _userManager.FindByEmailAsync("admin@test.com");
            var claims = await _userManager.GetClaimsAsync(user);
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
            _authService.SetCurrentUser(claimsPrincipal);

            // Act
            var result = await _authService.GetUserClaimsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == user.Email);
        }

        public void Dispose()
        {
            _context?.Dispose();
            _userManager?.Dispose();
        }
    }

    // Extension method to set the current user for testing
    public static class AuthenticationServiceExtensions
    {
        public static void SetCurrentUser(this IAuthenticationService authService, System.Security.Claims.ClaimsPrincipal user)
        {
            if (authService is AuthenticationService service)
            {
                var field = typeof(AuthenticationService).GetField("_currentUser", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(service, user);
            }
        }
    }
}
