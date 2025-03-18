using System.Security.Claims;
using Desktop.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace Desktop.Services;

public class UserManager : AuthenticationStateProvider
{
    private Dictionary<string, UserProfileDto> m_userProfileStore = new();

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Return not authenticated user.
        var user = new ClaimsPrincipal();
        return Task.FromResult(new AuthenticationState(user));
    }

    public async Task AuthenticateUser(UserProfileDto userProfile)
    {
        AddlUser(userProfile);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userProfile.Id),
        ], nameof(UserManager)));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        await Task.CompletedTask;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        => await Task.FromResult(m_userProfileStore.GetValueOrDefault(userId));

    private void AddlUser(UserProfileDto userProfile)
    {
        if (m_userProfileStore.ContainsKey(userProfile.Id))
        {
            m_userProfileStore.Remove(userProfile.Id);
        }
        m_userProfileStore.Add(userProfile.Id, userProfile);
    }
}