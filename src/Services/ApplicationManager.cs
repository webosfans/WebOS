using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Text.Json;
using WebOS.DTOs;
using Microsoft.JSInterop;

namespace WebOS.Services;

public class ApplicationManager
{
    private static ApplicationManager? Instance { get; set; }

    public ObservableCollection<AppDto> PinedApplications { get; } = new ();

    private readonly UserManager _userManager;

    public ApplicationManager(UserManager userManager)
    {
        Instance = this;

        _userManager = userManager;

        PinedApplications.Add(new AppDto("FeedReader", "/assets/img/app_icons/news@256x256.png", "https://feedreader.webos.fans")
        {
            DefaultWidth = 1280,
            DefaultHeight = 720,
        });
    }

    [JSInvokable]
    public static async Task<string?> OnMessage(string message)
    {
        var msg = JsonSerializer.Deserialize<RawMessage>(message);
        if (msg == null)
        {
            return null;
        }

        var appMessage = JsonSerializer.Deserialize<ApplicationMesssage>(msg.data);
        if (appMessage == null)
        {
            return null;
        }

        string? ret = null;
        if (Instance != null)
        {
            ret = await Instance.OnApplicationCommand(appMessage.message);
        }
        return JsonSerializer.Serialize(new ApplicationMesssage(appMessage.cookie, ret));
    }

    private async Task<string?> OnApplicationCommand(string? command)
    {
        switch (command)
        {
        case CmdRequestToken:
            var authState = await _userManager.GetAuthenticationStateAsync();
            return authState.User.FindFirst(ClaimTypes.Authentication)?.Value;

        default:
            return null;
        }
    }

    public record RawMessage(string origin, string data);

    private record ApplicationMesssage(string cookie, string? message);

    private const string CmdRequestToken = "request-token";
}