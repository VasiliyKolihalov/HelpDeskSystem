using System.Net;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Infrastructure.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
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

                _ => (int)HttpStatusCode.InternalServerError
            };

            await response.WriteAsync(JsonConvert.SerializeObject(new { message = exception.Message }));
        }
    }
}