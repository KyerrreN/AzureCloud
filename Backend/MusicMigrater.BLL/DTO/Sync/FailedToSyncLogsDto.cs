namespace MusicMigrater.BLL.DTO.Sync;

public record FailedToSyncLogsDto(
    int VkTotalCount,
    int SuccesfullyMigratedCount,
    List<SyncLogDto> FailedLogsList);
