using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskProductivity.Domain.Exceptions;

namespace TaskProductivity.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing request {Path}", context.Request.Path);

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                "One or more validation errors occurred.",
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                null
            ),
            BusinessRuleViolationException businessEx => (
                HttpStatusCode.BadRequest,
                "Business Rule Violation",
                businessEx.Message,
                null
            ),
            UnauthorizedResourceAccessException unauthorizedEx => (
                HttpStatusCode.Forbidden,
                "Access Denied",
                unauthorizedEx.Message,
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred while processing your request.",
                null
            )
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
