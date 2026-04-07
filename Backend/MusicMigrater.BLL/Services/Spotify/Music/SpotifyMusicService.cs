using Microsoft.Extensions.Logging;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.Spotify;
using MusicMigrater.BLL.Refit;
using MusicMigrater.BLL.Utils;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Enums;
using MusicMigrater.Domain.Exceptions;
using MusicMigrater.Domain.Logging;
using Refit;

namespace MusicMigrater.BLL.Services.Spotify.Music;

/// <summary>
/// Interface to work with Spotify API
/// </summary>
public interface ISpotifyMusicService
{
    Task<Result<SpotifyTracksResponse>> GetMySpotifyTracksAsync(int limit = 10, int offset = 0, CancellationToken cancellationToken = default);

    Task<Result<int>> SaveSpotifyTracksInLocalDbAsync(CancellationToken cancellationToken = default);
}

public class SpotifyMusicService(
    ISpotifyClient client,
    ILogger<SpotifyMusicService> logger,
    IMusicImportHelper importHelper) : ISpotifyMusicService
{
    public async Task<Result<SpotifyTracksResponse>> GetMySpotifyTracksAsync(int limit = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetMySpotifyTracksAsync(limit, offset, cancellationToken: cancellationToken);

            logger.LogSuccessfullyRetrievedTracksFromExternalService(ServiceConstants.Spotify, response.Total);

            return Result.Success(response);
        }
        catch (ApiException ex)
        {
            logger.LogErrorSendingRequestToExternalService(ServiceConstants.Spotify, (int)ex.StatusCode, ex.Message, ex);

            return Result.Failed<SpotifyTracksResponse>(ex.StatusCode.ToString());
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedError(ex);

            return Result.Failed<SpotifyTracksResponse>(ErrorConstants.UnexpectedError);
        }
    }

    public async Task<Result<int>> SaveSpotifyTracksInLocalDbAsync(CancellationToken cancellationToken = default)
    {
        var myTrackResult = await GetMySpotifyTracksAsync(1, 0, cancellationToken);

        if (!myTrackResult.IsSuccess)
        {
            return Result.Failed<int>(myTrackResult.Error);
        }

        var countOfTracks = myTrackResult.Value!.Total;

        var options = new BatchOptions<SpotifySingleTrackResponse>(
            ProviderName: ServiceConstants.Spotify,
            ProviderEnum: MusicProviderEnum.Spotify,
            BatchSize: 50,
            DelayBetweenBatchesMs: 200,
            FetchItemsFunc: async (offset, limit) =>
            {
                var response = await client.GetMySpotifyTracksAsync(limit, offset, cancellationToken);

                return response?.Items?.Select(x => x.Track).ToList() ?? [];
            },
            GetExternalIdFunc: x => x.Id
        );

        var result = await importHelper.ImportAsync(countOfTracks, options, cancellationToken);

        return result;
    }
}
