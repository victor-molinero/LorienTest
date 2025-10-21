using API_FrontEnd.Models;
using System.Net;
using System.Text.Json;

namespace API_FrontEnd.Infrastructure;
public sealed class HackerNewsClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions HnJson = new(JsonSerializerDefaults.Web);

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<int>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        using var resp = await _httpClient.GetAsync("beststories.json", ct);
        if (!resp.IsSuccessStatusCode) return [];

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var ids = await JsonSerializer.DeserializeAsync<List<int>>(stream, HnJson, ct);
        return ids ?? [];
    }

    public async Task<Story?> GetStoryAsync(int id, CancellationToken ct)
    {
        using var resp = await _httpClient.GetAsync($"item/{id}.json", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        if (!resp.IsSuccessStatusCode) return null;

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<Story>(stream, HnJson, ct);
    }
}
