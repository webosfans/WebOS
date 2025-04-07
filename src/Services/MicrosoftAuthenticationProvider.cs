using WebOS.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Text;
using JWT.Builder;
using System.Text.Json;

namespace WebOS.Services;

public class MicrosoftAuthenticationProvider
{
    private readonly MicrosoftOAuthOptions m_microsoftOAuthOptions;
    private readonly UserManager m_userManager;

    public string LoginUrl =>
        new StringBuilder($"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize")
            .Append($"?client_id={m_microsoftOAuthOptions.ClientId}")
            .Append($"&redirect_uri={m_microsoftOAuthOptions.CallbackUrl}")
            .Append("&response_type=id_token")
            .Append("&response_mode=fragment")
            .Append("&scope=openid")
            .Append("&nonce=webos")
            .ToString();

    public MicrosoftAuthenticationProvider(
        MicrosoftGraphApiOptions microsfotGraphApiOptions,
        MicrosoftOAuthOptions microsoftOAuthOptions,
        UserManager userManager,
        BrowserService browserService)
    {
        m_microsoftOAuthOptions = microsoftOAuthOptions;
        m_userManager = userManager;
    }

    public async Task LoginAsync(string callbackUrl)
    {
        // Get callback parameters.
        var fragment = callbackUrl.Substring(callbackUrl.IndexOf('#') + 1);
        var queries = QueryHelpers.ParseQuery(fragment);

        // Get id_token.
        var idToken = GetValue(queries, "id_token");
        if (string.IsNullOrEmpty(idToken))
        {
            throw new Exception($"Login failed (no id_token)");
        }

        // Parse the id which is jwt token.
        var claims = new JwtBuilder().DoNotVerifySignature().Decode<IDictionary<string, JsonElement>>(idToken);
        if (claims == null)
        {
            throw new Exception($"Login failed (invalid id_token)");
        }

        // Get sub.
        var sub = GetValue(claims, "sub");
        if (string.IsNullOrEmpty(sub))
        {
            throw new Exception($"Login failed (no sub)");
        }

        // Authenticate user.
        await m_userManager.AuthenticateUser(new UserProfileDto(sub), $"Bearer {idToken}");
    }

    private string? GetValue(IDictionary<string, StringValues> dict, string key)
            => dict.TryGetValue(key, out StringValues value) ? value.First() : null;

    private string? GetValue(IDictionary<string, JsonElement> dict, string key)
            => dict.TryGetValue(key, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.ToString() : null;
}