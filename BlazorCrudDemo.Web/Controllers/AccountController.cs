using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Web.Services;
using Microsoft.AspNetCore.Authentication;

namespace BlazorCrudDemo.Web.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuditService _auditService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger,
            IAuditService auditService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _auditService = auditService;
        }

        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Account is inactive. Please contact administrator.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Log successful login
                await _auditService.LogLoginAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], true);

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User logged in successfully: {Email}", email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return Redirect("/");
                }
            }

            if (result.IsLockedOut)
            {
                await _auditService.LogLoginAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], false, "Account locked out");
                ModelState.AddModelError(string.Empty, "Account is locked out.");
                return View();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _auditService.LogLogoutAsync(user.Id);
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            return Redirect("/");
        }
    }
}
