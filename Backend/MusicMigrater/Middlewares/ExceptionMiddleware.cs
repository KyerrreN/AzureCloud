using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.Domain.Exceptions;
using MusicMigrater.Domain.Logging;
using System.Net;

namespace MusicMigrater.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogInvalidParametersWarning(ex.Message);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var result = Result.Failed<bool>($"Invalid parameters: {ex.Message}");
            await context.Response.WriteAsJsonAsync(result);
        }
        catch (UnauthorizedException)
        {
            logger.LogWarning("Unauthorized");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var result = Result.Failed<bool>("Unauthorized");
            await context.Response.WriteAsJsonAsync(result);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedError(ex);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = Result.Failed<bool>("An unexpected error occurred on the server. Please try again later.");

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
