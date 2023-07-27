using System.Net;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        catch (Exception exception)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                _ => HandleInternalServerError(exception)
            };

            await response.WriteAsync(JsonConvert.SerializeObject(new { message = exception.Message }));
        }
    }

    private int HandleInternalServerError(Exception exception)
    {
        _logger.LogError(exception: exception, message: "Unexpected unhandled exception occurred");
        return (int)HttpStatusCode.InternalServerError;
    }
}