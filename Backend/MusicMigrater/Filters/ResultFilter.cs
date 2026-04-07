using MusicMigrater.BLL.DTO.Result;

namespace MusicMigrater.Filters;

public class ResultFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is IAppResult appResult)
        {
            if (appResult.IsSuccess)
            {
                return TypedResults.Ok(appResult);
            }

            return TypedResults.BadRequest(new
            {
                success = false,
                error = appResult.Error
            });
        }

        return result;
    }
}
