using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public class ZaloTokenService
{
    private readonly HttpClient _http;
    private readonly ZaloOptions _options;
    private readonly AppDbContext _db;

    public ZaloTokenService(HttpClient http, ZaloOptions options, AppDbContext db)
    {
        _http = http;
        _options = options;
        _db = db; // 🔥 FIX QUAN TRỌNG
    }

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
        token.ExpiredAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        await _db.SaveChangesAsync();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await GetTokenAsync();

        // ✅ còn hạn
        if (token != null && DateTime.UtcNow < token.ExpiredAt)
        {
            return token.AccessToken;
        }

        // 🔥 refresh token
        if (token != null && !string.IsNullOrEmpty(token.RefreshToken))
        {
            return await RefreshTokenAsync(token.RefreshToken);
        }

        // 🔥 lần đầu (PKCE)
        if (!string.IsNullOrEmpty(_options.Code) &&
            !string.IsNullOrEmpty(_options.CodeVerifier))
        {
            return await ExchangeCodeAsync();
        }

        throw new Exception("No token available. Need Code + CodeVerifier or RefreshToken.");
    }

    // 👉 Lấy token từ code (PKCE)
    private async Task<string> ExchangeCodeAsync()
    {
        var form = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["grant_type"] = "authorization_code",
            ["code"] = _options.Code,
            ["code_verifier"] = _options.CodeVerifier
        };

        var res = await _http.PostAsync(
            "https://oauth.zaloapp.com/v4/oa/access_token",
            new FormUrlEncodedContent(form)
        );

        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("EXCHANGE CODE RESPONSE:");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Exchange code failed: {content}");

        var obj = JObject.Parse(content);

        var accessToken = obj["access_token"]?.ToString();
        var refreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            throw new Exception("Invalid token response");

        // ✅ lưu DB luôn (KHÔNG chỉ log nữa)
        await SaveTokenAsync(accessToken, refreshToken, expiresIn);

        Console.WriteLine("🔥 INITIAL REFRESH TOKEN SAVED");

        return accessToken;
    }

    // 👉 Refresh token
    private async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var form = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        var res = await _http.PostAsync(
            "https://oauth.zaloapp.com/v4/oa/refresh_token",
            new FormUrlEncodedContent(form)
        );

        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("REFRESH RESPONSE:");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Refresh failed: {content}");

        var obj = JObject.Parse(content);

        var accessToken = obj["access_token"]?.ToString();
        var newRefreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(newRefreshToken))
            throw new Exception("Invalid refresh response");

        // 🔥 update DB (rotate token)
        await SaveTokenAsync(accessToken, newRefreshToken, expiresIn);

        Console.WriteLine("🔥 REFRESH TOKEN UPDATED");

        return accessToken;
    }
}