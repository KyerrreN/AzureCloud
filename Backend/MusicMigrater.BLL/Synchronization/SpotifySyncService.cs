using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.Sync;
using MusicMigrater.BLL.Refit;
using MusicMigrater.BLL.Utils;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Enums;
using MusicMigrater.Domain.Logging;
using Refit;
using System.Net;

namespace MusicMigrater.BLL.Synchronization;

/// <summary>
/// An interface to work with synchronization to Spotify
/// </summary>
public interface ISpotifySyncService
{
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    Task RunSyncPipelineAsync(CancellationToken ct = default);

    Task<Result<FailedToSyncLogsDto>> GetFailedLogsAsync(CancellationToken cancellationToken = default);
}

public class SpotifySyncService(
    AppDbContext dbContext,
    ISpotifyClient spotifyApi,
    ILogger<SpotifySyncService> logger) : ISpotifySyncService
{
    private const int DelayBetweenRequestsMs = 1000;

    public async Task<Result<FailedToSyncLogsDto>> GetFailedLogsAsync(CancellationToken cancellationToken = default)
    {
        var audiosSet = dbContext.Set<AudioEntity>();
        var syncLogsSet = dbContext.Set<SyncLogsEntity>();

        var totalTracks = await audiosSet
            .Where(x => x.Provider == MusicProviderEnum.Vk)
            .CountAsync(cancellationToken);

        var successfullyMigrated = await syncLogsSet
            .Where(x => x.Status == SyncStatusEnum.Success)
            .Select(x => x.AudioId)
            .Distinct()
            .CountAsync(cancellationToken);

        var successfulAudioIdsQuery = syncLogsSet
            .Where(x => x.Status == SyncStatusEnum.Success)
            .Select(x => x.AudioId);

        var failedLogsData = await syncLogsSet
            .Where(log => log.Status != SyncStatusEnum.Success && !successfulAudioIdsQuery.Contains(log.AudioId))
            .Join(audiosSet,
                log => log.AudioId,
                audio => audio.Id,
                (log, audio) => new { Log = log, Audio = audio })
            .OrderByDescending(x => x.Log.AttemptedAt) 
            .Select(x => new
            {
                x.Audio.Title,
                x.Audio.Artist,
                Provider = x.Log.TargetProvider,
                x.Log.ResultDetails,
                x.Log.AttemptedAt
            })
            .ToListAsync(cancellationToken);

        var failedLogs = failedLogsData
            .Select(x => new SyncLogDto(
                x.Title,
                x.Artist,
                x.Provider.ToString(),
                x.ResultDetails,
                x.AttemptedAt
            ))
            .ToList();

        var dto = new FailedToSyncLogsDto(
            VkTotalCount: totalTracks,
            SuccesfullyMigratedCount: successfullyMigrated,
            FailedLogsList: failedLogs
        );

        return dto;
    }

    public async Task RunSyncPipelineAsync(CancellationToken ct)
    {
        var rng = new Random();

        // macro pauses, avoids rate limiting at the cost of slower sync
        int macroRequestThreshold = rng.Next(300, 401);
        int macroRequestCount = 0;

        // micropauses, to also avoid rate limiting
        int requestThreshold = 80 + rng.Next(20);
        int requestCount = 0;

        var tracksToSync = await dbContext.Set<AudioEntity>()
            .Where(a => a.Provider == MusicProviderEnum.Vk)
            .Where(a => !dbContext.Set<SyncLogsEntity>().Any(log => 
                log.AudioId == a.Id &&
                log.TargetProvider == MusicProviderEnum.Spotify &&
                (log.Status == SyncStatusEnum.Success || log.Status == SyncStatusEnum.NotFound)))
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);

        if (tracksToSync.Count == 0)
        {
            logger.LogNoMoreTracksToSync();
            return;
        }

        logger.LogStartMigration(tracksToSync.Count);

        var urisToSave = new List<string>(30);
        var pendingLogs = new List<SyncLogsEntity>(50);

        foreach (var track in tracksToSync)
        {
            ct.ThrowIfCancellationRequested();

            var (cleanArtist, cleanTitle) = TrackCleaner.BuildSpotifySearchQuery(track.Artist, track.Title);
            var strictQuery = $"track:{cleanTitle} artist:{cleanArtist}";

            var currentLog = new SyncLogsEntity
            {
                AudioId = track.Id,
                TargetProvider = MusicProviderEnum.Spotify,
                AttemptedAt = DateTime.UtcNow,
            };

            try
            {
                if (macroRequestCount >= macroRequestThreshold)
                {
                    macroRequestCount = 0;
                    macroRequestThreshold = rng.Next(300, 401);

                    int sleepSeconds = rng.Next(420, 601);

                    logger.LogInitiateSyncSleep(macroRequestCount, sleepSeconds);

                    await Task.Delay(TimeSpan.FromMinutes(sleepSeconds), ct);
                }
                else if (requestCount >= requestThreshold)
                {
                    requestCount = 0;
                    requestThreshold = 80 + rng.Next(20);

                    await Task.Delay(rng.Next(5000, 10000), ct);
                }

                var searchResponse = await spotifyApi.SearchAsync(strictQuery, cancellationToken: ct);
                
                requestCount++;
                macroRequestCount++;

                var foundTrack = searchResponse.Tracks.Items.FirstOrDefault();

                if (foundTrack is not null)
                {
                    logger.LogFoundTrackOnSpotify(track.Title, track.Artist);

                    currentLog.Status = SyncStatusEnum.Success;
                    urisToSave.Add($"spotify:track:{foundTrack.Id}");
                }
                else
                {
                    logger.LogCouldntFindTrackOnSpotify(track.Title, track.Artist);

                    currentLog.Status = SyncStatusEnum.NotFound;
                    currentLog.ResultDetails = $"Couldn't find track on spotify. Title: {track.Title}, Artist: {track.Artist}";
                }
            }
            catch (ApiException ex)
            {
                logger.LogErrorSendingRequestToExternalService(ServiceConstants.Spotify, (int)ex.StatusCode, ex.Content, ex);
                currentLog.Status = SyncStatusEnum.Error;
                currentLog.ResultDetails = $"API error: {ex.StatusCode}";

                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    logger.LogHitRateLimiter();
                    pendingLogs.Add(currentLog);
                    await FlushBatchAsync(urisToSave, pendingLogs, ct);
                    return;
                }

                requestCount++;
                macroRequestCount++;
            }
            catch (Exception ex)
            {
                logger.LogUnexpectedError(ex);
                currentLog.Status = SyncStatusEnum.Error;
                currentLog.ResultDetails = ErrorConstants.UnexpectedError;

                requestCount++;
                macroRequestCount++;
            }

            pendingLogs.Add(currentLog);

            if (urisToSave.Count >= 30 || pendingLogs.Count >= 50)
            {
                await FlushBatchAsync(urisToSave, pendingLogs, ct);
            }

            int finalDelay = DelayBetweenRequestsMs + rng.Next(100, 500);

            await Task.Delay(finalDelay, ct);
        }

        if (urisToSave.Count != 0 || pendingLogs.Count != 0)
        {
            await FlushBatchAsync(urisToSave, pendingLogs, ct);
        }

        logger.LogTrackMigrationSuccess();
    }

    private async Task FlushBatchAsync(List<string> uris, List<SyncLogsEntity> logs, CancellationToken ct)
    {
        if (uris.Count != 0)
        {
            try
            {
                await spotifyApi.AddToLikedAsync([.. uris], cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogFailedToSaveBatchWhileMigrating(uris.Count, ServiceConstants.Spotify, ex);

                foreach (var log in logs.Where(l => l.Status == SyncStatusEnum.Success))
                {
                    log.Status = SyncStatusEnum.Error;
                    log.ResultDetails = "Failed to save to Spotify library";
                }
            }
            uris.Clear();
        }

        if (logs.Count != 0)
        {
            dbContext.SyncLogs.AddRange(logs);
            await dbContext.SaveChangesAsync(ct);

            dbContext.ChangeTracker.Clear();
            logs.Clear();
        }
    }
}
