using System.Text.Json.Serialization;

namespace API_FrontEnd.Contracts;
public class BestStoryDto
{
    [JsonPropertyName("title")] 
    public string? Title { get; set; }

    [JsonPropertyName("uri")] 
    public string? Uri { get; set; }

    [JsonPropertyName("postedBy")] 
    public string? PostedBy { get; set; }

    [JsonPropertyName("time")] 
    public string? Time { get; set; }

    [JsonPropertyName("score")] 
    public int Score { get; set; }

    [JsonPropertyName("commentCount")] 
    public int CommentCount { get; set; }

}
