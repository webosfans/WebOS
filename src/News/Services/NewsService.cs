using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using News.DTOs;

namespace News.Services;

public class NewsService
{
    private HttpClient? m_apiClient;

    private readonly AuthenticationStateProvider m_authenticationStateProvider;

    public NewsService(AuthenticationStateProvider authenticationStateProvider)
    {
        m_authenticationStateProvider = authenticationStateProvider;
    }

    private async Task<HttpClient> GetHttpClientAsync()
    {
        if (m_apiClient == null)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("https://api.news.webos.fans/") };
            var authentication = (await m_authenticationStateProvider.GetAuthenticationStateAsync()).User.FindFirst(ClaimTypes.Authentication)?.Value;
            if (!string.IsNullOrEmpty(authentication))
            {
                httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authentication);
            }
            m_apiClient = httpClient;
        }
        return m_apiClient;
    }

    public async Task<IEnumerable<NewsDto>> GetFollowedNewsAsync(int page)
        => await (await GetHttpClientAsync()).GetFromJsonAsync<IEnumerable<NewsDto>>($"latest?limit=20&offset={page * 20}") ?? [];
}