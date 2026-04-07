using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicMigrater.BLL.DTO.Audio;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.VkMusic;
using MusicMigrater.BLL.Refit;
using MusicMigrater.BLL.Utils;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Constants;
using MusicMigrater.Domain.Enums;
using MusicMigrater.Domain.Logging;
using Refit;

namespace MusicMigrater.BLL.Services.VK.MusicService;

/// <summary>
/// Interface for working with Vk Music
/// </summary>
public interface IVkMusicService
{
    Task<Result<int>> GetMusicCountAsync(CancellationToken cancellationToken = default);

    Task<Result<int>> AddMusicToLocalDbAsync(CancellationToken cancellationToken = default);

    Task<Result<List<AudioModel>>> GetAudioFromLocalDbAsync(int offset, int count, MusicProviderEnum provider, CancellationToken cancellationToken = default);
}

public class VkMusicService(
    ILogger<VkMusicService> logger,
    AppDbContext dbContext,
    IVkClient vkClient,
    IMusicImportHelper importHelper) : IVkMusicService
{
    public async Task<Result<int>> AddMusicToLocalDbAsync(CancellationToken cancellationToken = default)
    {
        var countResult = await GetMusicCountAsync(cancellationToken);

        if (!countResult.IsSuccess)
        {
            return Result.Failed<int>(countResult.Error);
        }

        var token = await dbContext.Set<UserSettingsEntity>()
            .Where(x => x.Code == UserSettingsConstants.VkUserTokenCode)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(token))
        {
            logger.LogVkTokenMissingError();

            return Result.Failed<int>("Token is missing, application is in inconsistent state");
        }

        var options = new BatchOptions<VkAudioItemModel>(
            ProviderName: ServiceConstants.Vk,
            ProviderEnum: MusicProviderEnum.Vk,
            BatchSize: 200,
            DelayBetweenBatchesMs: 100,
            FetchItemsFunc: async (offset, limit) =>
            {
                var response = await vkClient.GetMusicAsync(token, limit, offset, cancellationToken: cancellationToken);
                return response?.Response?.Items?.ToList() ?? [];
            },
            GetExternalIdFunc: x => x.Id.ToString()
        );

        var result = await importHelper.ImportAsync(countResult.Value, options, cancellationToken);

        // set status to true for client
        var entitySet = dbContext.Set<UserSettingsEntity>();

        var musicMigratedEntity = await entitySet.Where(
            x => x.Code == UserSettingsConstants.VkMusicMigratedCode &&
            x.Value == UserSettingsConstants.VkMusicMigratedFalseValue
        ).FirstOrDefaultAsync(cancellationToken);

        if (musicMigratedEntity is null)
        {
            musicMigratedEntity = new UserSettingsEntity
            {
                Code = UserSettingsConstants.VkMusicMigratedCode,
                Value = UserSettingsConstants.VkMusicMigratedTrueValue
            };

            entitySet.Add(musicMigratedEntity);
        }
        else
        {
            musicMigratedEntity.Value = UserSettingsConstants.VkMusicMigratedTrueValue;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<List<AudioModel>>> GetAudioFromLocalDbAsync(int offset, int count, MusicProviderEnum provider, CancellationToken cancellationToken = default)
    {
        if (offset < 0 || count < 0)
        {
            return Result.Failed<List<AudioModel>>($"{nameof(offset)}/{nameof(count)} parameters cannot be negative");
        }

        var result = await dbContext.Set<AudioEntity>()
            .Where(x => x.Provider == provider)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return result.Adapt<List<AudioModel>>();
    }

    public async Task<Result<int>> GetMusicCountAsync(CancellationToken cancellationToken = default)
    {
        var token = await dbContext.Set<UserSettingsEntity>()
            .Where(x => x.Code == UserSettingsConstants.VkUserTokenCode)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(token))
        {
            logger.LogVkTokenMissingError();

            return Result.Failed<int>("Token is missing, application is in inconsistent state");
        }

        try
        {
            var response = await vkClient.GetMusicAsync(token, 1, cancellationToken: cancellationToken);

            return Result<int>.Success(response.Response.Count);
        }
        catch (ApiException e)
        {
            logger.LogRequestError(ErrorConstants.ErrorSendToVk, e);

            return Result<int>.Failed(ErrorConstants.ErrorSendToVk);
        }
        catch (Exception e)
        {
            logger.LogRequestError(ErrorConstants.UnexpectedError, e);

            return Result<int>.Failed(ErrorConstants.ErrorSendToVk);
        }
    }
}
