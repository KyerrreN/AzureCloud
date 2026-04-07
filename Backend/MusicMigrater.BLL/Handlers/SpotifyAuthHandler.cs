using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicMigrater.BLL.Options;
using MusicMigrater.BLL.Refit;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Exceptions;
using MusicMigrater.Domain.Logging;
using Refit;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MusicMigrater.BLL.Handlers;

public class SpotifyAuthHandler(
    IServiceScopeFactory scopeFactory,
    IOptions<SpotifyOptions> options,
    ILogger<SpotifyAuthHandler> logger) : DelegatingHandler
{
    // limits token refreshes
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var spotifyClient = scope.ServiceProvider.GetRequiredService<ISpotifyAuthClient>();

        var settings = await dbContext.Set<UserSettingsEntity>()
            .Where(x => x.Code == UserSettingsConstants.SpotifyAccessToken
                     || x.Code == UserSettingsConstants.SpotifyRefreshToken)
            .ToListAsync(cancellationToken);

        var accessToken = settings.FirstOrDefault(x => x.Code == UserSettingsConstants.SpotifyAccessToken);
        var refreshToken = settings.FirstOrDefault(x => x.Code == UserSettingsConstants.SpotifyRefreshToken);

        if (refreshToken is null)
        {
            logger.LogUnauthorizedInServiceError(ServiceConstants.Spotify);
            throw new UnauthorizedException("Spotify is not linked (missing refresh token)");
        }

        bool needsRefresh = accessToken is null ||
                            accessToken.Expires is null ||
                            accessToken.Expires <= DateTimeOffset.UtcNow.AddSeconds(30);

        if (needsRefresh)
        {
            await Semaphore.WaitAsync(cancellationToken);
            try
            {
                // concurrency safety
                dbContext.ChangeTracker.Clear();

                settings = await dbContext.Set<UserSettingsEntity>()
                    .Where(x => x.Code == UserSettingsConstants.SpotifyAccessToken
                             || x.Code == UserSettingsConstants.SpotifyRefreshToken)
                    .ToListAsync(cancellationToken);

                accessToken = settings.FirstOrDefault(x => x.Code == UserSettingsConstants.SpotifyAccessToken);
                refreshToken = settings.FirstOrDefault(x => x.Code == UserSettingsConstants.SpotifyRefreshToken);

                if (refreshToken is null)
                {
                    logger.LogUnauthorizedInServiceError(ServiceConstants.Spotify);
                    throw new UnauthorizedException("Spotify is not linked");
                }

                if (accessToken is null || accessToken.Expires is null || accessToken.Expires <= DateTimeOffset.UtcNow.AddSeconds(30))
                {
                    logger.LogTokenRefreshingInformation(ServiceConstants.Spotify);

                    var opt = options.Value;
                    var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opt.ClientId}:{opt.ClientSecret}"));

                    var data = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken!.Value }
                    };

                    var newTokens = await spotifyClient.RefreshTokenAsync(data, authHeader);

                    if (accessToken is null)
                    {
                        accessToken = new UserSettingsEntity { Code = UserSettingsConstants.SpotifyAccessToken };
                        dbContext.Add(accessToken);
                    }
                    else
                    {
                        dbContext.Update(accessToken);
                    }

                    accessToken.Value = newTokens.AccessToken;
                    accessToken.Expires = DateTimeOffset.UtcNow.AddSeconds(newTokens.ExpiresIn);

                    if (!string.IsNullOrEmpty(newTokens.RefreshToken))
                    {
                        refreshToken.Value = newTokens.RefreshToken;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogTokenRefreshedSuccessfully(ServiceConstants.Spotify);
                }
            }
            catch (ApiException ex)
            {
                // remove tokens if they're expired
                if (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    await dbContext.Set<UserSettingsEntity>()
                        .Where(x => x.Code == UserSettingsConstants.SpotifyAccessToken
                                 || x.Code == UserSettingsConstants.SpotifyRefreshToken)
                        .ExecuteDeleteAsync(cancellationToken);

                    throw new UnauthorizedException("Tokens have expired");
                }

                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        if (accessToken is null)
        {
            throw new UnauthorizedException("Failed to acquire access token");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

        return await base.SendAsync(request, cancellationToken);
    }
}
