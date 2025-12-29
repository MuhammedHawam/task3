using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartnersHub.InfraBase.Application.Common.Exceptions;

namespace PartnersHub.InfraBase.Apis.Common;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses
/// </summary>
public class GlobalExceptionMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment) {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        } catch (Exception ex) {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception) {
        _logger.LogError(exception, 
            "Unhandled exception occurred: {Message} | Path: {Path} | Method: {Method}",
            exception.Message, 
            context.Request.Path, 
            context.Request.Method);

        var (statusCode, message, shouldExposeDetails) = MapExceptionToResponse(exception);

        var errorResponse = new {
            HttpCode = statusCode,
            Status = statusCode >= 500 ? "Error" : "Failed",
            Error = new {
                Message = message,
                Details = shouldExposeDetails || _environment.IsDevelopment() ? exception.Message : null,
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }

    private (int StatusCode, string Message, bool ExposeDetails) MapExceptionToResponse(Exception exception) {
        return exception switch {
            // Custom domain exceptions
            ValidationException => 
                (StatusCodes.Status400BadRequest, "Validation error", true),

            NotFoundException => 
                (StatusCodes.Status404NotFound, "Resource not found", true),

            // Business rule violations
            InvalidOperationException => 
                (StatusCodes.Status400BadRequest, "Business rule violation", true),

            // Standard validation errors
            ArgumentException or ArgumentNullException => 
                (StatusCodes.Status400BadRequest, "Invalid request data", true),

            // Not found
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Resource not found", true),

            // Unauthorized
            UnauthorizedAccessException => 
                (StatusCodes.Status401Unauthorized, "Unauthorized access", false),

            // Database concurrency
            DbUpdateConcurrencyException => 
                (StatusCodes.Status409Conflict, "Resource was modified by another user", false),

            // Database constraint violations
            DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate key") == true =>
                (StatusCodes.Status409Conflict, "Duplicate record", false),

            // Generic database errors
            DbUpdateException => 
                (StatusCodes.Status400BadRequest, "Database operation failed", false),

            // Generic server error
            _ => (StatusCodes.Status500InternalServerError, 
                  "An unexpected error occurred while processing your request", false)
        };
    }
}