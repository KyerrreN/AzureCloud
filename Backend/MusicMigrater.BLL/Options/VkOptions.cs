namespace MusicMigrater.BLL.Options;

public class VkOptions
{
    public const string SectionName = "vkOptions";

    public string Uri { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;
}
