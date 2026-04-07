using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.UserSettings;
using MusicMigrater.BLL.DTO.VkMusic;
using MusicMigrater.BLL.Refit;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Logging;
using Refit;

namespace MusicMigrater.BLL.Services.Settings;

/// <summary>
/// Service to work with user settings
/// </summary>
public interface IUserSettingsService
{
    Task<Result<bool>> IsRegisteredByProviderAsync(string provider, CancellationToken cancellationToken = default);

    Task<Result<bool>> SaveVkAuthDataAsync(VkTokenDto dto, CancellationToken cancellationToken = default);

    Task<Result<IsAuthenticatedInServicesDto>> CheckServicesAuthentication(CancellationToken cancellationToken = default);
}

public class UserSettingsService(
    AppDbContext dbContext, 
    IVkClient vkClient,
    ILogger<UserSettingsService> logger) : IUserSettingsService
{
    public async Task<Result<IsAuthenticatedInServicesDto>> CheckServicesAuthentication(CancellationToken cancellationToken = default)
    {
        var set = dbContext.Set<UserSettingsEntity>();

        var vkToken = await set.AnyAsync(s => s.Code == UserSettingsConstants.VkUserTokenCode, cancellationToken);

        var spotifyToken = await set.AnyAsync(s => s.Code == UserSettingsConstants.SpotifyAccessToken, cancellationToken);

        var musicImportStatus = await set.AnyAsync(s => s.Code == UserSettingsConstants.VkMusicMigratedCode
            && s.Value == UserSettingsConstants.VkMusicMigratedTrueValue, cancellationToken);

        return new IsAuthenticatedInServicesDto(vkToken, spotifyToken, musicImportStatus);
    }

    public async Task<Result<bool>> IsRegisteredByProviderAsync(string provider, CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<UserSettingsEntity>()
            .AnyAsync(x => x.Code == provider, cancellationToken);
    }

    public async Task<Result<bool>> SaveVkAuthDataAsync(VkTokenDto dto, CancellationToken cancellationToken = default)
    {
        var isAlreadyAuthenticated = await dbContext
            .Set<UserSettingsEntity>()
            .AnyAsync(x => x.Code == UserSettingsConstants.VkUserTokenCode, cancellationToken);

        if (isAlreadyAuthenticated)
        {
            logger.LogUserAlreadyAuthenticatedWarning();

            return Result<bool>.Failed("User is already authenticated");
        }

        try
        {
            var response = await vkClient.GetMusicAsync(dto.Token, 1, 0, cancellationToken: cancellationToken);

            if (response.Response is null)
            {
                logger.LogTokenError();

                return Result<bool>.Failed("Token is invalid, couldn't authenticate user");
            }
        }
        catch (ApiException ex)
        {
            logger.LogCouldntReachServiceError(ServiceConstants.Vk, ex);

            return Result<bool>.Failed("Couldn't reach VK");
        }
        catch (Exception ex)
        {
            logger.LogRequestError(ErrorConstants.UnexpectedError, ex);

            return Result<bool>.Failed(ErrorConstants.UnexpectedError);
        }

        var userSettingsEntity = new UserSettingsEntity
        {
            Code = UserSettingsConstants.VkUserTokenCode,
            Value = dto.Token,
            Expires = dto.ExpiresIn > 0
                ? DateTime.UtcNow.AddSeconds(dto.ExpiresIn)
                : DateTime.MaxValue
        };

        var musicMigrationStatus = new UserSettingsEntity
        {
            Code = UserSettingsConstants.VkMusicMigratedCode,
            Value = UserSettingsConstants.VkMusicMigratedFalseValue,
        };

        dbContext.Set<UserSettingsEntity>()
            .AddRange(userSettingsEntity, musicMigrationStatus);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogRequestError(ErrorConstants.ErrorWhileSavingToDb, ex);

            return Result<bool>.Failed(ErrorConstants.ErrorWhileSavingToDb);
        }

        return true;
    }
}
