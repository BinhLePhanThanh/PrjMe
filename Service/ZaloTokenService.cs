using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;
using System.Text;

public static class PkceHelper
{
    public static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
public class ZaloTokenService
{
    private readonly HttpClient _http;
    private readonly ZaloOptions _options;
    private readonly AppDbContext _db;

    private const string TOKEN_URL = "https://oauth.zaloapp.com/v4/access_token";

    public ZaloTokenService(
        HttpClient http,
        IOptions<ZaloOptions> options,
        AppDbContext db)
    {
        _http = http;
        _options = options.Value;
        _db = db;
    }

    // ================= DB =================

    private async Task<ZaloTokenEntity?> GetTokenAsync()
    {
        return await _db.ZaloTokens.FirstOrDefaultAsync();
    }

    private async Task SaveTokenAsync(string accessToken, string refreshToken, int expiresIn)
    {
        var token = await _db.ZaloTokens.FirstOrDefaultAsync();

        if (token == null)
        {
            token = new ZaloTokenEntity();
            _db.ZaloTokens.Add(token);
        }

        token.AccessToken = accessToken;
        token.RefreshToken = refreshToken;

        // trừ buffer 60s tránh expire race condition
        token.ExpiredAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        await _db.SaveChangesAsync();
    }

    // ================= MAIN =================

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await GetTokenAsync();

        // ✅ còn hạn
        if (token != null && DateTime.UtcNow < token.ExpiredAt)
        {
            return token.AccessToken;
        }

        // 🔥 refresh nếu có
        if (token != null && !string.IsNullOrEmpty(token.RefreshToken))
        {
            return await RefreshTokenAsync(token.RefreshToken);
        }

        throw new Exception("No token available. Need to authorize via /api/zalo/login first.");
    }

    // ================= CALLBACK (PKCE) =================

    public async Task ExchangeCodeFromCallbackAsync(string code, string codeVerifier)
    {
        var form = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["redirect_uri"] = _options.RedirectUri // 🔥 bắt buộc
        };

        var request = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL);
        request.Headers.Add("secret_key", _options.SecretKey);
        request.Content = new FormUrlEncodedContent(form);

        var res = await _http.SendAsync(request);
        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("===== ZALO CALLBACK RESPONSE =====");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP ERROR: {res.StatusCode} - {content}");

        var obj = JObject.Parse(content);

        // 🔥 bắt lỗi từ Zalo
        if (obj["error"] != null)
        {
            throw new Exception($"Zalo error: {content}");
        }

        var accessToken = obj["access_token"]?.ToString();
        var refreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            throw new Exception($"Invalid token response: {content}");
        }

        await SaveTokenAsync(accessToken, refreshToken, expiresIn);

        Console.WriteLine("🔥 CALLBACK TOKEN SAVED");
    }

    // ================= REFRESH =================

    private async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var form = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL);
        request.Headers.Add("secret_key", _options.SecretKey);
        request.Content = new FormUrlEncodedContent(form);

        var res = await _http.SendAsync(request);
        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("===== ZALO REFRESH RESPONSE =====");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP ERROR: {res.StatusCode} - {content}");

        var obj = JObject.Parse(content);

        if (obj["error"] != null)
        {
            throw new Exception($"Zalo refresh error: {content}");
        }

        var accessToken = obj["access_token"]?.ToString();
        var newRefreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(newRefreshToken))
        {
            throw new Exception($"Invalid refresh response: {content}");
        }

        await SaveTokenAsync(accessToken, newRefreshToken, expiresIn);

        Console.WriteLine("🔥 REFRESH TOKEN UPDATED");

        return accessToken;
    }
}