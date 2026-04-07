namespace MusicMigrater.BLL.DTO.Sync;

public record SyncLogDto(
    string Title,
    string Artist,
    string TargetProvider,
    string? ResultDetails,
    DateTime AttemptedAt);
