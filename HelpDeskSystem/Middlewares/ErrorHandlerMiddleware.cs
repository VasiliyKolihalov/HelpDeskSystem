using System.Net;
using HelpDeskSystem.Exceptions;
using Newtonsoft.Json;

namespace HelpDeskSystem.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            response.StatusCode = error switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                
                _ => (int)HttpStatusCode.InternalServerError
            };

            string result = JsonConvert.SerializeObject(new { message = error.Message });
            await response.WriteAsync(result);
        }
    }
}