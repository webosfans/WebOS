using System.Security.Claims;
using System.Text.Json.Serialization;
using Desktop.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace Desktop.Services;

public class UserManager : AuthenticationStateProvider
{
    private readonly IHttpClientFactory m_httpClientFactory;
    private Dictionary<string, UserProfileDto> m_userProfileStore = new();

    public UserManager(IHttpClientFactory httpClientFactory)
    {
        m_httpClientFactory = httpClientFactory;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Return not authenticated user.
        var user = new ClaimsPrincipal();
        return Task.FromResult(new AuthenticationState(user));
    }

    public async Task AuthenticateUser(UserProfileDto userProfile)
    {
        AddUser(userProfile);

        SubscribeUser(userProfile);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userProfile.Id),
        ], nameof(UserManager)));
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