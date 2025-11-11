using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OtpNet;
using QRCoder;
using BlazorCrudDemo.Data.Models;
using Microsoft.Extensions.Logging;

namespace BlazorCrudDemo.Web.Services
{
    public class TwoFactorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TwoFactorService> _logger;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public TwoFactorService(
            UserManager<ApplicationUser> userManager,
            ILogger<TwoFactorService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Setup2FAResult> GenerateTwoFactorSetup(string email, string appName = "BlazorCrudDemo")
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new Setup2FAResult { Success = false, ErrorMessage = "User not found" };
            }

            // Generate a secret key
            var key = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(key).Replace("=", "");

            // Store the secret key in the user's record
            await _userManager.SetAuthenticationTokenAsync(user, "BlazorCrudDemo", "AuthenticatorKey", base32Secret);

            // Generate setup code and QR code
            var formattedKey = FormatKey(base32Secret);
            var authenticatorUri = string.Format(
                AuthenticatorUriFormat,
                appName,
                user.Email,
                base32Secret);

            return new Setup2FAResult
            {
                Success = true,
                SharedKey = base32Secret,
                FormattedKey = formattedKey,
                AuthenticatorUri = authenticatorUri
            };
        }

        public async Task<bool> VerifyTwoFactorCode(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Get the secret key from the user's record
            var authenticatorKey = await _userManager.GetAuthenticationTokenAsync(user, "BlazorCrudDemo", "AuthenticatorKey");
            if (string.IsNullOrEmpty(authenticatorKey)) return false;

            // Verify the code
            var totp = new Totp(Base32Encoding.ToBytes(authenticatorKey));
            bool isValid = totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));

            if (isValid)
            {
                // If this is the first successful verification, enable 2FA
                if (!await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                }
                return true;
            }

            return false;
        }

        public async Task<bool> DisableTwoFactor(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (result.Succeeded)
            {
                // Remove the authenticator key
                await _userManager.RemoveAuthenticationTokenAsync(user, "BlazorCrudDemo", "AuthenticatorKey");
                return true;
            }
            return false;
        }

        public async Task<bool> IsTwoFactorEnabled(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            return await _userManager.GetTwoFactorEnabledAsync(user);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        public string GenerateQrCode(string email, string authenticatorUri)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);
                return "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
            }
        }
    }

    public class Setup2FAResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SharedKey { get; set; }
        public string? FormattedKey { get; set; }
        public string? AuthenticatorUri { get; set; }
    }
}
