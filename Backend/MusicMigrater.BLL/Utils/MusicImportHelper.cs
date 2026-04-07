using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Logging;
using Refit;

namespace MusicMigrater.BLL.Utils;

/// <summary>
/// Engine to support importing audio from external services
/// </summary>
public interface IMusicImportHelper
{
    Task<Result<int>> ImportAsync<TSource>(int totalCount, BatchOptions<TSource> options, CancellationToken ct = default);
}

public class MusicImportHelper(AppDbContext dbContext, ILogger<MusicImportHelper> logger) : IMusicImportHelper
{
    public async Task<Result<int>> ImportAsync<TSource>(
    int totalCount,
    BatchOptions<TSource> options,
    CancellationToken ct = default)
    {
        int newlyAdded = 0;
        logger.LogStartBatch(options.ProviderName);

        try
        {
            for (int offset = 0; offset < totalCount; offset += options.BatchSize)
            {
                var items = await options.FetchItemsFunc(offset, options.BatchSize);
                if (items is null || items.Count == 0) break;

                var batchExternalIds = items.Select(options.GetExternalIdFunc).ToList();

                var existingIds = await dbContext.Audios
                    .Where(a => a.Provider == options.ProviderEnum && batchExternalIds.Contains(a.ExternalId))
                    .Select(a => a.ExternalId)
                    .ToListAsync(ct);

                var filteredItems = items
                    .Where(i => !existingIds.Contains(options.GetExternalIdFunc(i)))
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    var entities = filteredItems.Adapt<List<AudioEntity>>();
                    await dbContext.Audios.AddRangeAsync(entities, ct);
                    await dbContext.SaveChangesAsync(ct);
                    newlyAdded += entities.Count;
                }

                dbContext.ChangeTracker.Clear();

                logger.LogDataBatch(newlyAdded, totalCount, options.ProviderName);

                if (options.DelayBetweenBatchesMs > 0)
                    await Task.Delay(options.DelayBetweenBatchesMs, ct);
            }

            return Result.Success(newlyAdded);
        }
        catch (ApiException e)
        {
            logger.LogRequestError(ErrorConstants.ErrorSendToProvider, e);

            return Result.Failed<int>(ErrorConstants.ErrorSendToProvider);
        }
        catch (Exception e)
        {
            logger.LogRequestError(ErrorConstants.UnexpectedError, e);

            return Result.Failed<int>(ErrorConstants.UnexpectedError);
        }
    }
}
