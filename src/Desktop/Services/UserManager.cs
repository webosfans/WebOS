using System.Security.Claims;
using Desktop.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace Desktop.Services;

public class UserManager : AuthenticationStateProvider
{
    private readonly BrowserService m_browserService;
    private readonly IHttpClientFactory m_httpClientFactory;
    private Dictionary<string, UserProfileDto> m_userProfileStore = new();

    public UserManager(IHttpClientFactory httpClientFactory, BrowserService browserService)
    {
        m_httpClientFactory = httpClientFactory;
        m_browserService = browserService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user;
        var userId = await m_browserService.SessionLoadItemAsync("user-id");
        var userToken = await m_browserService.SessionLoadItemAsync("user-token");
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userToken))
        {
            user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Authentication, userToken),
            ], nameof(UserManager)));
        }
        else
        {
            user = new ClaimsPrincipal();
        }
        return new AuthenticationState(user);
    }

    public async Task AuthenticateUser(UserProfileDto userProfile, string? token)
    {
        AddUser(userProfile);

        SubscribeUser(userProfile);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userProfile.Id),
        ], nameof(UserManager)));

        if (!string.IsNullOrEmpty(token))
        {
            user.AddIdentity(new ClaimsIdentity([
                new Claim(ClaimTypes.Authentication, token),
            ]));

            await m_browserService.SessionSaveItemAsync("user-id", token);
            await m_browserService.SessionSaveItemAsync("user-token", token);
        }
        else
        {
            await m_browserService.SessionLoadAndCleanItemAsync("user-id");
            await m_browserService.SessionLoadAndCleanItemAsync("user-token");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        await Task.CompletedTask;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        => await Task.FromResult(m_userProfileStore.GetValueOrDefault(userId));

    private void AddUser(UserProfileDto userProfile)
    {
        if (m_userProfileStore.ContainsKey(userProfile.Id))
        {
            m_userProfileStore.Remove(userProfile.Id);
        }
        m_userProfileStore.Add(userProfile.Id, userProfile);
    }

    private async void SubscribeUser(UserProfileDto userProfile)
    {
        if (userProfile.Id == Consts.GuestUserId)
        {
            userProfile.DisplayName = "Guest";
            userProfile.StateHasChanged();
        }
        else
        {
            var graphClient = m_httpClientFactory.CreateClient("ms-graph-api");
            var binaryData = await graphClient.GetAsync("me/photos/240x240/$value");
            if (!binaryData.IsSuccessStatusCode || binaryData.Content.Headers.ContentType == null)
            {
                return;
            }

            var avatarBytes = await binaryData.Content.ReadAsByteArrayAsync();
            userProfile.AvatarUrl = $"data:{binaryData.Content.Headers.ContentType};base64,{Convert.ToBase64String(avatarBytes)}";
            userProfile.StateHasChanged();
        }
    }
}