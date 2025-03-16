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

    public async Task LoginAsync()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Name, "Guest"),
        ], nameof(LocalAuthenticationStateProvider)));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        await Task.CompletedTask;
    }
}