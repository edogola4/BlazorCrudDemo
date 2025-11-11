using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorCrudDemo.Web.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Security Headers - only add if not already present
            SafeAddHeader(context, "X-Content-Type-Options", "nosniff");
            SafeAddHeader(context, "X-Frame-Options", "DENY");
            SafeAddHeader(context, "X-XSS-Protection", "1; mode=block");
            SafeAddHeader(context, "Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Generate a cryptographically secure random nonce for each request
            var nonce = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
            
            // Add nonce to the context items so it can be used in views and scripts
            context.Items["CspNonce"] = nonce;
            
            // Add WebSocket upgrade headers if this is a WebSocket request
            if (context.WebSockets.IsWebSocketRequest)
            {
                SafeAddHeader(context, "Upgrade", "websocket");
                SafeAddHeader(context, "Connection", "Upgrade");
            }
            
            // Add nonce as a response header for script tags to access
            context.Response.Headers["CSP-Nonce"] = nonce;
            
            // Add nonce to the response headers for JavaScript to access
            context.Response.Headers["X-Content-Security-Policy-Nonce"] = nonce;
            
            // Content Security Policy for Blazor Server with WebSocket support
            var csp = new[]
            {
                "default-src 'self'",
                "script-src 'self' 'nonce-" + nonce + "' 'strict-dynamic' 'unsafe-eval' 'unsafe-inline' https: http:",
                "style-src 'self' 'nonce-" + nonce + "' 'unsafe-inline' https: http:",
                "img-src 'self' data: https: http:",
                "font-src 'self' https: http: data:",
                "connect-src 'self' wss: ws: https: http: wss://localhost:5120 ws://localhost:5120 https://localhost:5120 http://localhost:5120",
                "frame-src 'self' https: http:",
                "object-src 'none'",
                "base-uri 'self'",
                "form-action 'self'",
                "frame-ancestors 'none'",
                "block-all-mixed-content",
                "upgrade-insecure-requests"
            };

            // Set the CSP header
            var cspHeaderValue = string.Join("; ", csp) + ";";
            SafeAddHeader(context, "Content-Security-Policy", cspHeaderValue);
            
            // Set Permissions Policy
            SafeAddHeader(context, "Permissions-Policy", "camera=(), geolocation=(), microphone=()");
            
            // HSTS - Only add in production
            if (!_configuration.GetValue<bool>("DisableHSTS") && !context.Response.Headers.ContainsKey("Strict-Transport-Security"))
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }
            
            await _next(context);
        }
        
        private static void SafeAddHeader(HttpContext context, string key, string value)
        {
            if (!context.Response.Headers.ContainsKey(key))
            {
                context.Response.Headers[key] = value;
            }
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
