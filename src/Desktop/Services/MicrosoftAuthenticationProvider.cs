using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Desktop.Services;

public class MicrosoftAuthenticationProvider
{
    private string ClientId { get; init; }

    private string CallbackUrl { get; init; }

    private string CodeVerifier { get; init; }

    private string CodeChallenge { get; init; }

    private HttpClient HttpClient { get; init; }

    private LocalAuthenticationStateProvider AuthenticationStateProvider { get; init; }

    public string LoginUrl =>
        new StringBuilder($"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize")
            .Append($"?client_id={ClientId}")
            .Append($"&redirect_uri={CallbackUrl}")
            .Append("&response_type=code")
            .Append("&response_mode=fragment")
            .Append("&scope=files.readwrite+user.read")
            .Append("&nonce=webos")
            .Append($"&code_challenge={CodeChallenge}")
            .Append("&code_challenge_method=S256")
            .ToString();

    public MicrosoftAuthenticationProvider(string clientId, string callbackUrl, HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        ClientId = clientId;
        CallbackUrl = callbackUrl;
        CodeVerifier = GenerateCodeVerifier();
        CodeChallenge = GenerateCodeChallenge(CodeVerifier);
        HttpClient = httpClient;
        AuthenticationStateProvider = (LocalAuthenticationStateProvider)authenticationStateProvider;
    }

    public async Task LoginAsync(string callbackUrl)
    {
        // Get callback parameters.
        var fragment = callbackUrl.Substring(callbackUrl.IndexOf('#') + 1);
        var queries = QueryHelpers.ParseQuery(fragment);

        // Get code.
        var code = GetValue(queries, "code");
        if (string.IsNullOrEmpty(code))
        {
            throw new Exception($"Login failed (unknown code)");
        }

        // Redeem token.
        var response = await HttpClient.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", CallbackUrl),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code_verifier", CodeVerifier),
        ]));
        response = response.EnsureSuccessStatusCode();
        var redeemResponse = await response.Content.ReadFromJsonAsync<RedeemTokenResponse>();
        if (string.IsNullOrEmpty(redeemResponse?.AccessToken))
        {
            throw new Exception($"Login failed (no token redeemed)");
        }

        // Add token to http client.
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", redeemResponse.AccessToken);

        // Get user profile.
        response = await HttpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
        response = response.EnsureSuccessStatusCode();
        var userProfile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        if (userProfile == null)
        {
            throw new Exception($"Login failed (get user profile failed)");
        }
        await AuthenticationStateProvider.AuthenticateUser(userProfile.DisplayName);
    }

    private string? GetValue(IDictionary<string, StringValues> dict, string key)
            => dict.TryGetValue(key, out StringValues value) ? value.First() : null;

    private static string GenerateCodeVerifier()
    {
        // TODO: should generate random string.
        return "i6pznjw2qqwot8g9pfdsswpbqsy5lxjd6fmj7c73jkuhjf4fcor17pib8rwtca1c9el6ngnrfus8i6awzysky2cexvasa2see4ftfv8vdg9bkg8dcgr4vidf2ibexs7v";
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var b64Hash = Convert.ToBase64String(hash);
        var code = Regex.Replace(b64Hash, "\\+", "-");
        code = Regex.Replace(code, "\\/", "_");
        code = Regex.Replace(code, "=+$", "");
        return code;
    }

    private class RedeemTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private class UserProfileResponse
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;
    }
}