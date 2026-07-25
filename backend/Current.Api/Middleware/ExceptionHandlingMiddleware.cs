using System.Text.Json;
using Current.Api.Common.Exceptions;
using Current.Api.DTOs.Payments;

namespace Current.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, responseBody) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception processing {RequestMethod} {RequestPath}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with {StatusCode} for {RequestMethod} {RequestPath}",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(responseBody, JsonOptions));
    }

    private (int StatusCode, object ResponseBody) MapException(Exception exception)
    {
        return exception switch
        {
            PaymentException paymentException => (
                StatusCodes.Status400BadRequest,
                new PaymentErrorResponse
                {
                    Code = paymentException.Code,
                    Message = paymentException.Message,
                }),
            UnauthorizedAccessException unauthorizedException => (
                StatusCodes.Status401Unauthorized,
                new { message = unauthorizedException.Message }),
            InvalidCredentialsException credentialsException => (
                StatusCodes.Status401Unauthorized,
                new { message = credentialsException.Message }),
            EmailNotVerifiedException emailNotVerifiedException => (
                StatusCodes.Status403Forbidden,
                new { message = emailNotVerifiedException.Message }),
            InvalidVerificationCodeException invalidVerificationCodeException => (
                StatusCodes.Status400BadRequest,
                new { message = invalidVerificationCodeException.Message }),
            DuplicateEmailException duplicateEmailException => (
                StatusCodes.Status409Conflict,
                new { message = duplicateEmailException.Message }),
            InvalidOperationException invalidOperationException => (
                StatusCodes.Status400BadRequest,
                new { message = invalidOperationException.Message }),
            KeyNotFoundException keyNotFoundException => (
                StatusCodes.Status404NotFound,
                new { message = keyNotFoundException.Message }),
            _ => (
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred.",
                }),
        };
    }
}
