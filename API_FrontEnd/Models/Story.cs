using System.Text.Json.Serialization;

namespace API_FrontEnd.Models;
public class Story
{
    [JsonPropertyName("id")] 
    public int Id { get; set; }

    [JsonPropertyName("title")] 
    public string? Title { get; set; }

    [JsonPropertyName("url")] 
    public string? Url { get; set; }

    [JsonPropertyName("by")] 
    public string? By { get; set; }

    [JsonPropertyName("time")] 
    public long Time { get; set; }

    [JsonPropertyName("score")] 
    public int Score { get; set; }

    [JsonPropertyName("descendants")] 
    public int? Descendants { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("dead")] 
    public bool? Dead { get; set; }

    [JsonPropertyName("deleted")]
    public bool? Deleted { get; set; }

}
