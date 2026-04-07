namespace MusicMigrater.BLL.Options;

public class SpotifyOptions
{
    public const string SectionName = "SpotifyOptions";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;
}
