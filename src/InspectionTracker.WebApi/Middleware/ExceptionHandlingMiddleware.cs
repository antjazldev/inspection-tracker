using System.Text.Json;
using InspectionTracker.Application.Exceptions;

namespace InspectionTracker.WebApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            BusinessValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message }));
    }
}
