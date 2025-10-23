using Microsoft.AspNetCore.Mvc;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BlazorCrudDemo.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        accessToken = result.AccessToken,
                        refreshToken = result.RefreshToken,
                        expiresAt = result.ExpiresAt,
                        user = result.User
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API login");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        user = result.User
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API registration");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var result = await _authService.LogoutAsync();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API logout");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        accessToken = result.AccessToken,
                        refreshToken = result.RefreshToken,
                        expiresAt = result.ExpiresAt
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();

                if (user != null)
                {
                    return Ok(new
                    {
                        success = true,
                        user = user
                    });
                }

                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpGet("test-jwt")]
        [Authorize]
        public IActionResult TestJwt()
        {
            return Ok(new
            {
                success = true,
                message = "JWT authentication is working!",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
