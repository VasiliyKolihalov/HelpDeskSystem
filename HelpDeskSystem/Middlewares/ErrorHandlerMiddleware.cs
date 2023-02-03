using System.ComponentModel.DataAnnotations;
using System.Net;
using HelpDeskSystem.Exceptions;

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
                BadRequestException => (int)HttpStatusCode.BadRequest,
                ValidationException => (int)HttpStatusCode.BadRequest,
                ForbiddenException => (int)HttpStatusCode.Forbidden,

                _ => (int)HttpStatusCode.InternalServerError
            };

            await response.WriteAsJsonAsync(new { message = error.Message });
        }
    }
}