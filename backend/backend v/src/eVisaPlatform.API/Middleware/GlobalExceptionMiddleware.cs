using eVisaPlatform.Application.Common;
using System.Net;
using System.Text.Json;

namespace eVisaPlatform.API.Middleware;

/// <summary>
/// Global unhandled exception handler — maps domain exception types to
/// appropriate HTTP status codes so service methods never need to return
/// HTTP concerns directly.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            // 400 Bad Request
            ArgumentException or
            ArgumentNullException or
            InvalidOperationException       => (HttpStatusCode.BadRequest,   exception.Message),

            // 401 Unauthorized
            UnauthorizedAccessException     => (HttpStatusCode.Unauthorized, "You are not authorised to perform this action."),

            // 403 Forbidden  (use a custom type if you add it)
            // ForbiddenException           => (HttpStatusCode.Forbidden, exception.Message),

            // 404 Not Found
            KeyNotFoundException            => (HttpStatusCode.NotFound,     exception.Message),

            // 409 Conflict (duplicate resources etc.)
            // ConflictException            => (HttpStatusCode.Conflict, exception.Message),

            // 500 fall-through — hide internal detail from client
            _                               => (HttpStatusCode.InternalServerError,
                                                "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var response    = ApiResponse.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
