using News.DTOs;

namespace News.Services;

public class NewsService
{
    public async Task<IEnumerable<NewsDto>> GetFollowedNewsAsync(int page)
    {
        await Task.Delay(1000);

        var news = new List<NewsDto>();
        for (var i = page * 50; i < (page + 1) * 50; ++i)
        {
            news.Add(new NewsDto($"publishid-{i}", "This is news content: {i}"));
        }
        return news;
    }
}