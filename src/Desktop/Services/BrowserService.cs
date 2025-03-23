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

    private readonly IJSRuntime m_jsRuntime;

    public BrowserService(IJSRuntime jsRuntime)
    {
        m_jsRuntime = jsRuntime;
    }

    public Task SessionSaveItemAsync(string key, string value)
    {
        return m_jsRuntime.InvokeVoidAsync("SessionSaveItem", key, value).AsTask();
    }

    public Task<string> SessionLoadItemAsync(string key)
    {
        return m_jsRuntime.InvokeAsync<string>("SessionLoadItem", key).AsTask();
    }

    public Task<string> SessionLoadAndCleanItemAsync(string key)
    {
        return m_jsRuntime.InvokeAsync<string>("SessionLoadAndCleanItem", key).AsTask();
    }
}