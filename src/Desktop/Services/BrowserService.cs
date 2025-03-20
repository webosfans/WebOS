using Microsoft.JSInterop;

namespace Desktop.Services;

public class BrowserService
{
    public static event EventHandler? OnResize;

    [JSInvokable]
    public static void OnBrowserResize()
    {
        OnResize?.Invoke(null, EventArgs.Empty);
    }
}