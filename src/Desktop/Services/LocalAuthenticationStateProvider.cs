using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Desktop.Services;

public class LocalAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Return not authenticated user.
        var user = new ClaimsPrincipal();
        return Task.FromResult(new AuthenticationState(user));
    }

    public async Task AuthenticateUser(string userName)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Name, userName),
        ], nameof(LocalAuthenticationStateProvider)));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        await Task.CompletedTask;
    }

    public async Task LoginAsync()
    {
        await AuthenticateUser("Guest");
    }
}