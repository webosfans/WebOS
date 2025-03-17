using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Desktop.Services;

public class MicrosoftAuthenticationProvider
{
    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly MicrosoftGraphApiOptions m_microsoftGraphApiOptions;
    private readonly MicrosoftOAuthOptions m_microsoftOAuthOptions;
    private readonly LocalAuthenticationStateProvider m_authenticationStateProvider;
    private readonly HttpClient m_defaultHttpClient;
    private readonly string m_codeVerifier;
    private readonly string m_codeChallenge;

    public string LoginUrl =>
        new StringBuilder($"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize")
            .Append($"?client_id={m_microsoftOAuthOptions.ClientId}")
            .Append($"&redirect_uri={m_microsoftOAuthOptions.CallbackUrl}")
            .Append("&response_type=code+id_token")
            .Append("&response_mode=fragment")
            .Append("&scope=files.readwrite+user.read+openid")
            .Append("&nonce=webos")
            .Append($"&code_challenge={m_codeChallenge}")
            .Append("&code_challenge_method=S256")
            .ToString();

    public MicrosoftAuthenticationProvider(
        IHttpClientFactory httpClientFactory,
        MicrosoftGraphApiOptions microsfotGraphApiOptions,
        MicrosoftOAuthOptions microsoftOAuthOptions,
        AuthenticationStateProvider authenticationStateProvider,
        HttpClient defaultHttpClient)
    {
        m_httpClientFactory = httpClientFactory;
        m_microsoftGraphApiOptions = microsfotGraphApiOptions;
        m_microsoftOAuthOptions = microsoftOAuthOptions;
        m_authenticationStateProvider = (LocalAuthenticationStateProvider)authenticationStateProvider;
        m_defaultHttpClient = defaultHttpClient;
        m_codeVerifier = GenerateCodeVerifier();
        m_codeChallenge = GenerateCodeChallenge(m_codeVerifier);
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
            throw new Exception($"Login failed (no code)");
        }

        // Get id_token.
        var idToken = GetValue(queries, "id_token");
        if (string.IsNullOrEmpty(idToken))
        {
            throw new Exception($"Login failed (no id_token)");
        }

        // Redeem token.
        var response = await m_defaultHttpClient.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", m_microsoftOAuthOptions.ClientId),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", m_microsoftOAuthOptions.CallbackUrl),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code_verifier", m_codeVerifier),
        ]));
        response = response.EnsureSuccessStatusCode();
        var redeemResponse = await response.Content.ReadFromJsonAsync<RedeemTokenResponse>();
        if (string.IsNullOrEmpty(redeemResponse?.AccessToken))
        {
            throw new Exception($"Login failed (no token redeemed)");
        }

        // Update authentication token.
        m_microsoftGraphApiOptions.BearerToken = redeemResponse.AccessToken;

        // Get a new ms graph client
        var graphClient = m_httpClientFactory.CreateClient("ms-graph-api");

        // Get user profile.
        var userProfile = await graphClient.GetFromJsonAsync<UserProfileResponse>("me");
        if (userProfile == null)
        {
            throw new Exception($"Login failed (get user profile failed)");
        }
        await m_authenticationStateProvider.AuthenticateUser(userProfile.DisplayName);
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