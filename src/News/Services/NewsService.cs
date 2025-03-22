using System.Text;
using News.DTOs;

namespace News.Services;

public class NewsService
{
    public async Task<IEnumerable<NewsDto>> GetFollowedNewsAsync(int page)
    {
        await Task.Delay(2000);

        var news = new List<NewsDto>();
        if (page < 5)
        {
            var rand = new Random();
            for (var i = page * 50; i < (page + 1) * 50; ++i)
            {
                var sb = new StringBuilder();
                for (var lines = 0; lines < rand.Next(10) + 1; ++lines)
                {
                    sb.AppendLine($"This is new content line: {lines + 1}<br/>");
                }
                news.Add(new NewsDto($"publishid-{i}", sb.ToString()));
            }
        }
        return news;
    }
}