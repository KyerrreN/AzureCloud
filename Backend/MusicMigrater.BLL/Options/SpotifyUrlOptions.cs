namespace MusicMigrater.BLL.Options;

public class SpotifyUrlOptions
{
    public const string SectionName = "SpotifyUrlOptions";

    public string AuthUrl { get; set; } = string.Empty;

    public string ClientUrl { get; set; } = string.Empty;
}
