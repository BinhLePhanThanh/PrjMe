using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class ZaloTokenService
{
    private readonly HttpClient _http;
    private readonly ZaloOptions _options;

    private string _accessToken;
    private DateTime _expiredAt;

    public ZaloTokenService(HttpClient http, ZaloOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // ✅ dùng cache
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiredAt)
        {
            return _accessToken;
        }

        var body = new
        {
            app_id = _options.AppId,
            app_secret = _options.AppSecret,
            grant_type = "client_credentials"
        };

        var json = JsonConvert.SerializeObject(body);

        var response = await _http.PostAsync(
            "https://oauth.zaloapp.com/v4/oa/access_token",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine("TOKEN RESPONSE:");
        Console.WriteLine(content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token API error: {content}");
        }

        var obj = JObject.Parse(content);

        // ✅ access_token
        var token = obj["access_token"]?.ToString();

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception($"Invalid token response: {content}");
        }

        // ✅ expires_in (nullable safe)
        int expiresIn = 3600; // default fallback

        if (obj["expires_in"] != null && obj["expires_in"].Type != JTokenType.Null)
        {
            expiresIn = obj["expires_in"].Value<int>();
        }
        else
        {
            Console.WriteLine("⚠️ expires_in is null → using default 3600s");
        }

        _accessToken = token;
        _expiredAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        return _accessToken;
    }
}