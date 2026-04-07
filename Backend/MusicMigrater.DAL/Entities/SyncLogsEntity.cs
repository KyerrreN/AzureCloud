using MusicMigrater.Domain.Enums;

namespace MusicMigrater.DAL.Entities;

public class SyncLogsEntity
{
    public int Id { get; set; }

    public int AudioId { get; set; }
    public AudioEntity Audio { get; set; } = null!;

    public MusicProviderEnum TargetProvider { get; set; }

    public SyncStatusEnum Status { get; set; }

    public string? ResultDetails { get; set; }

    public DateTime AttemptedAt { get; set; }
}
