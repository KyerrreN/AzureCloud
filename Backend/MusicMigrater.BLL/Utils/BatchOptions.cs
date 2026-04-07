using MusicMigrater.Domain.Enums;

namespace MusicMigrater.BLL.Utils;

public record BatchOptions<TSource>(
    string ProviderName,
    MusicProviderEnum ProviderEnum,
    int BatchSize,
    int DelayBetweenBatchesMs,
    Func<int, int, Task<List<TSource>>> FetchItemsFunc,
    Func<TSource, string> GetExternalIdFunc
);
