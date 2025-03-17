using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Desktop;
using Desktop.Services;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, LocalAuthenticationStateProvider>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new MicrosoftAuthenticationProvider(
    "bb0f2870-69fd-469f-9093-5e1716ec5e60",
    sp.GetRequiredService<NavigationManager>().ToAbsoluteUri("login/microsoft").ToString(),
    sp.GetRequiredService<HttpClient>(),
    sp.GetRequiredService<AuthenticationStateProvider>()));

await builder.Build().RunAsync();
