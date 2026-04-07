using System.Diagnostics;
using MusicMigrater.Domain.Logging;

namespace MusicMigrater.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = Stopwatch.GetTimestamp();
        var path = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        logger.LogRequestStarted(method, path);

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            logger.LogRequestFinished(method, path, elapsed.TotalMilliseconds, context.Response.StatusCode);
        }
    }
}
