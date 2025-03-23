using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using News.Services;

public static class NewsExtensions
{
    public static IServiceCollection AddNewsServices(this IServiceCollection services, IServiceProvider parentServiceProvider)
    {
        services.AddSingleton(sp => new NewsService(parentServiceProvider.GetRequiredService<AuthenticationStateProvider>()));
        return services;
    }
}