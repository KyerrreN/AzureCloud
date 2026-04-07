using System.Text.RegularExpressions;

namespace MusicMigrater.BLL.Utils;

/// <summary>
/// A class to clean track name and artist name to make Spotify search more accurate
/// </summary>
public static partial class TrackCleaner
{
    private static readonly Regex BracketsRegex = BracketsRegexHighPerformance();

    private static readonly Regex NoiseWordsRegex = NoiseWordsRegexHighPerformance();

    private static readonly Regex FeatRegex = FeatRegexHighPerformance();

    private static readonly char[] separator = [',', '&'];

    [GeneratedRegex(@"\s+")]
    private static partial Regex CleanerTitleHighPerformance();

    [GeneratedRegex(@"(?i)\b(feat\.?|ft\.?)\s+.*", RegexOptions.Compiled, "en-150")]
    private static partial Regex FeatRegexHighPerformance();

    [GeneratedRegex(@"[\(\[].*?[\)\]]", RegexOptions.Compiled)]
    private static partial Regex BracketsRegexHighPerformance();

    [GeneratedRegex(@"(?i)\b(official|video|audio|lyric|lyrics|bass boosted|slowed|reverb|8d|hq|cover|remix)\b", RegexOptions.Compiled, "en-150")]
    private static partial Regex NoiseWordsRegexHighPerformance();

    public static (string CleanArtist, string CleanTitle) BuildSpotifySearchQuery(string artist, string title)
    {
        var cleanTitle = BracketsRegex.Replace(title, string.Empty);
        cleanTitle = FeatRegex.Replace(cleanTitle, string.Empty);
        cleanTitle = NoiseWordsRegex.Replace(cleanTitle, string.Empty);

        var primaryArtist = artist.Split(separator, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? artist;

        cleanTitle = CleanerTitleHighPerformance().Replace(cleanTitle, " ").Trim();
        primaryArtist = primaryArtist.Trim();

        return (primaryArtist, cleanTitle);
    }
}
