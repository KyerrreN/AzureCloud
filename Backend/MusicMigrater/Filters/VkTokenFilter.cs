using Microsoft.EntityFrameworkCore;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Logging;

namespace MusicMigrater.Filters;

public class VkTokenFilter(AppDbContext dbContext, ILogger<VkTokenFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var hasValidToken = await dbContext.Set<UserSettingsEntity>()
            .AnyAsync(x => x.Code == UserSettingsConstants.VkUserTokenCode
                           && !string.IsNullOrEmpty(x.Value)
                           && (x.Expires == null || x.Expires > DateTime.UtcNow));

        if (!hasValidToken)
        {
            logger.LogUnauthorizedInServiceError(ServiceConstants.Vk);

            return Results.Json(
                Result.Failed<bool>("Unauthorized in VK"),
                statusCode: StatusCodes.Status401Unauthorized
                );
        }

        return await next(context);
    }
}
