using API_FrontEnd.Contracts;
using API_FrontEnd.Models;

namespace API_FrontEnd.Extensions
{
    public static class BestStoryExtensions
    {
        public static BestStoryDto? MapToBestStoryDto(this Story story)
        {
            if (story is null) return null;

            var when = DateTimeOffset.FromUnixTimeSeconds(story.Time).UtcDateTime;

            return new BestStoryDto
            {
                Title = story.Title,
                Uri = story.Url,
                PostedBy = story.By,
                Time = when.ToString("yyyy-MM-ddTHH:mm:ssK"), 
                Score = story.Score,
                CommentCount = story.Descendants ?? 0
            };
        }
    }
}
