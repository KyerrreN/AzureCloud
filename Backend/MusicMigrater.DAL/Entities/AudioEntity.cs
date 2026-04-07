using MusicMigrater.Domain.Enums;

namespace MusicMigrater.DAL.Entities;

public class AudioEntity
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = null!;

    public MusicProviderEnum Provider { get; set; }

    public string Artist { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Duration { get; set; }

    public DateTime CreatedAt { get; set; }
}
