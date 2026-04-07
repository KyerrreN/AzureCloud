using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.Options;
using MusicMigrater.BLL.Refit;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Logging;
using Refit;
using System.Text;

namespace MusicMigrater.BLL.Services.Spotify.Auth;

/// <summary>
/// Service for working with Spotify Auth
/// </summary>
public interface ISpotifyAuthService
{
    Task<Result<bool>> ExchangeCodeAndSaveTokenAsync(string code, CancellationToken cancellationToken = default);
}

public class SpotifyAuthService(
    AppDbContext dbContext,
    IOptions<SpotifyOptions> spotifyOptions,
    ISpotifyAuthClient client,
    ILogger<SpotifyAuthService> logger) : ISpotifyAuthService
{
    public async Task<Result<bool>> ExchangeCodeAndSaveTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        var opt = spotifyOptions.Value;

        try
        {
            var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opt.ClientId}:{opt.ClientSecret}"));

            var data = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", opt.RedirectUri }
            };

            var tokens = await client.ExchangeCodeAsync(data, authHeader);

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn);

            await SaveToken(UserSettingsConstants.SpotifyAccessToken, tokens.AccessToken, expiresAt, cancellationToken);
            await SaveToken(UserSettingsConstants.SpotifyRefreshToken, tokens.RefreshToken, null, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogTokenSavedSuccessfully(ServiceConstants.Spotify);

            return Result.Success(true);
        }
        catch (ApiException ex)
        {
            logger.LogErrorSendingRequestToExternalService(ServiceConstants.Spotify, (int)ex.StatusCode, ex.Content, ex);

            return Result.Failed<bool>($"Error while reaching spotify, StatusCode: {ex.StatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedError(ex);

            return Result.Failed<bool>(ErrorConstants.UnexpectedError);
        }
    }

    private async Task SaveToken(string code, string value, DateTimeOffset? expires, CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Set<UserSettingsEntity>()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (setting is null)
        {
            dbContext.Set<UserSettingsEntity>().Add(new UserSettingsEntity
            {
                Code = code,
                Value = value,
                Expires = expires,
            });
        }
        else
        {
            setting.Value = value;
            setting.Expires = expires;
        }
    }
}
