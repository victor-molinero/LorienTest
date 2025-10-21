using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BestStoriesClient
{
    public class Program
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7298/") // ✅ Adjust if your API runs elsewhere
        };

        public static async Task Main()
        {
           
            Console.Write("How many best stories do you want to view? (default 10): ");
            string? input = Console.ReadLine();

            int n = 10;
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int parsed) && parsed > 0)
                n = parsed;

            Console.WriteLine($"\nFetching top {n} stories...\n");

            try
            {
                var response = await httpClient.GetAsync($"best-stories/{n}");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var stories = JsonSerializer.Deserialize<List<BestStory>>(json, options);

                if (stories is { Count: > 0 })
                {
                    int count = 1;
                    foreach (var story in stories)
                    {
                        Console.WriteLine($"Top: {count}.\nTitle: {story.Title}");
                        Console.WriteLine($"Link: {story.Uri}");
                        Console.WriteLine($"PostedBy: {story.PostedBy}");
                        Console.WriteLine($"When: {story.Time}");
                        Console.WriteLine($"Score: {story.Score} points.\nComments: {story.CommentCount}\n\n");
                        count++;
                    }
                }
                else
                {
                    Console.WriteLine("No stories found or API returned empty list.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error fetching stories: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    public class BestStory
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("uri")] public string? Uri { get; set; }
        [JsonPropertyName("postedBy")] public string? PostedBy { get; set; }
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("score")] public int Score { get; set; }
        [JsonPropertyName("commentCount")] public int CommentCount { get; set; }
    }
}
