namespace MusicMigrater.DAL.Entities;

public class UserSettingsEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTimeOffset? Expires { get; set; }
}
