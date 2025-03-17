using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Desktop;
using Desktop.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<MicrosoftGraphApiOptions>();
builder.Services.AddSingleton(sp => new MicrosoftOAuthOptions("bb0f2870-69fd-469f-9093-5e1716ec5e60", sp.GetRequiredService<NavigationManager>().ToAbsoluteUri("login/microsoft").ToString()));
builder.Services.AddHttpClient("ms-graph-api", (sp, client) =>
{
    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
    var options = sp.GetRequiredService<MicrosoftGraphApiOptions>();
    if (!string.IsNullOrEmpty(options.BearerToken))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.BearerToken);
    }
});
builder.Services.AddScoped<AuthenticationStateProvider, LocalAuthenticationStateProvider>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<MicrosoftAuthenticationProvider>();

await builder.Build().RunAsync();
