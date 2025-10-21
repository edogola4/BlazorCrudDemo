using Serilog;
using System.Net;
using System.Text.Json;

namespace BlazorCrudDemo.Web.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<GlobalExceptionHandlerMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception with full details
        _logger.Error(exception, "Unhandled exception occurred: {Message}", exception.Message);

        // Set appropriate status code based on exception type
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        // Create error response
        var errorResponse = new
        {
            error = new
            {
                message = GetUserFriendlyMessage(exception),
                code = statusCode.ToString(),
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            }
        };

        // Log additional context for debugging
        _logger.Warning("Exception details - Request: {Method} {Path}, UserAgent: {UserAgent}, IP: {RemoteIP}",
            context.Request.Method,
            context.Request.Path,
            context.Request.Headers.UserAgent,
            context.Connection.RemoteIpAddress);

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid request parameters provided.",
            UnauthorizedAccessException => "You don't have permission to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            InvalidOperationException => "The operation could not be completed.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}
