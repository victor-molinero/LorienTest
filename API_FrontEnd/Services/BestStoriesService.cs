using API_FrontEnd.Contracts;
using API_FrontEnd.Extensions;
using API_FrontEnd.Infrastructure;
using API_FrontEnd.Models;
using Microsoft.Extensions.Caching.Memory;

public sealed class BestStoriesService
{
    private readonly HackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _cache;

    private readonly SemaphoreSlim _parallelGate = new(initialCount: 8, maxCount: 8); 

    private static readonly TimeSpan BestIdsCacheTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StoryCacheTtl = TimeSpan.FromMinutes(5);

    public BestStoriesService(HackerNewsClient hackerNewsClient, IMemoryCache cache)
    {
        _hackerNewsClient = hackerNewsClient;
        _cache = cache;
    }

    public async Task<IReadOnlyList<BestStoryDto>> GetTopBestStoriesAsync(int n, CancellationToken ct)
    {
        var bestIds = await _cache.GetOrCreateAsync("hn:beststories", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = BestIdsCacheTtl;
            return await _hackerNewsClient.GetBestStoryIdsAsync(ct);
        }) ?? [];

        if (bestIds.Count == 0) return Array.Empty<BestStoryDto>();

        var slice = bestIds.Take(n).ToArray();

        var tasks = slice.Select(id => GetStoryWithCache(id, ct)).ToArray();
        var stories = await Task.WhenAll(tasks);

        return stories
            .Where(s => s is not null)
            .Select(s => s!.MapToBestStoryDto()!) 
            .ToList();
    }

    private async Task<Story?> GetStoryWithCache(int id, CancellationToken ct)
    {
        var cacheKey = $"hn:story:{id}";
        if (_cache.TryGetValue(cacheKey, out Story? cached) && cached is not null)
            return cached;

        await _parallelGate.WaitAsync(ct);
        try
        {
            var story = await _hackerNewsClient.GetStoryAsync(id, ct);
            if (story is not null)
            {
                _cache.Set(cacheKey, story, StoryCacheTtl);
            }
            return story;
        }
        finally
        {
            _parallelGate.Release();
        }
    }
    //private static BestStoryDto? MapToBestStoryDto(Story story)
    //{
    //    if (story is null) return null;

    //    var when = DateTimeOffset.FromUnixTimeSeconds(story.Time).UtcDateTime;

    //    return new BestStoryDto
    //    {
    //        Title = story.Title,
    //        Uri = story.Url,
    //        PostedBy = story.By,
    //        Time = when.ToString("yyyy-MM-ddTHH:mm:ssK"), 
    //        Score = story.Score,
    //        CommentCount = story.Descendants ?? 0
    //    };
    //}
}
