using MusicMigrater.Domain.Enums;

namespace MusicMigrater.BLL.DTO.Sync;

public record SyncJobResult(
    int AudioId,
    SyncStatusEnum Status,
    string? SpotifyId,
    string? Details);
