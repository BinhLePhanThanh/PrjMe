using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text;

public class ZaloTokenService
{
    private readonly HttpClient _http;
    private readonly ZaloOptions _options;
    private readonly AppDbContext _db;

    private const string TOKEN_URL = "https://oauth.zaloapp.com/v4/oa/access_token";

    public ZaloTokenService(
        HttpClient http,
        IOptions<ZaloOptions> options,
        AppDbContext db)
    {
        _http = http;
        _options = options.Value;
        _db = db;
    }

    // ================= EXCHANGE =================

    public async Task ExchangeCodeAsync(string code)
    {
        var form = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["grant_type"] = "authorization_code",
            ["code"] = code
        };

        var request = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL);
        request.Headers.Add("secret_key", _options.SecretKey);
        request.Content = new FormUrlEncodedContent(form);

        var res = await _http.SendAsync(request);
        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("===== OA TOKEN RESPONSE =====");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP ERROR: {res.StatusCode} - {content}");

        var obj = JObject.Parse(content);

        if (obj["error"] != null)
            throw new Exception($"Zalo error: {content}");

        var accessToken = obj["access_token"]?.ToString();
        var refreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken))
            throw new Exception("No access_token");

        await SaveTokenAsync(accessToken, refreshToken, expiresIn);
    }

    // ================= PUBLIC (CHO SERVICE KHÁC DÙNG) =================

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await _db.ZaloTokens.FirstOrDefaultAsync();

        if (token == null)
            throw new Exception("No OA token. Call /api/zalo/login first.");

        if (DateTime.UtcNow < token.ExpiredAt)
            return token.AccessToken;

        return await RefreshTokenAsync(token.RefreshToken);
    }

    // ================= REFRESH =================

    private async Task<string> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new Exception("Missing refresh_token");

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

        Console.WriteLine("===== REFRESH TOKEN RESPONSE =====");
        Console.WriteLine(content);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP ERROR: {res.StatusCode} - {content}");

        var obj = JObject.Parse(content);

        if (obj["error"] != null)
            throw new Exception($"Refresh error: {content}");

        var accessToken = obj["access_token"]?.ToString();
        var newRefreshToken = obj["refresh_token"]?.ToString();
        int expiresIn = obj["expires_in"]?.Value<int>() ?? 3600;

        if (string.IsNullOrEmpty(accessToken))
            throw new Exception("No access_token in refresh");

        await SaveTokenAsync(accessToken, newRefreshToken, expiresIn);

        return accessToken;
    }

    // ================= DB =================

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

        Console.WriteLine("🔥 TOKEN SAVED/UPDATED");
    }
}