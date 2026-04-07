namespace MusicMigrater.BLL.DTO.VkMusic;

/// <summary>
/// Record-wrapper for a response
/// </summary>
/// <param name="Response">Response model</param>
public record VkAudioResponseModel(VkAudioDataModel Response);

/// <summary>
/// Record that wraps audio metadata and data model
/// </summary>
/// <param name="Count">Overall count of audio</param>
/// <param name="Items">Audio models </param>
public record VkAudioDataModel(int Count, IReadOnlyList<VkAudioItemModel> Items);

/// <summary>
/// Record that wraps audio data
/// </summary>
/// <param name="Artist"></param>
/// <param name="Title"></param>
/// <param name="Duration"></param>
/// <param name="Id"></param>
public record VkAudioItemModel(string Artist, string Title, int Duration, long Id);
