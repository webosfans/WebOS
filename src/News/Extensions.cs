using Microsoft.Extensions.DependencyInjection;
using News.Services;

public static class NewsExtensions
{
    public static IServiceCollection AddNewsServices(this IServiceCollection services)
    {
        services.AddSingleton<NewsService>();
        return services;
    }
}