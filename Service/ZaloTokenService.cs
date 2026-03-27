using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

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

        dynamic result = JsonConvert.DeserializeObject(content);

        _accessToken = result.access_token;
        int expiresIn = result.expires_in;

        _expiredAt = DateTime.UtcNow.AddSeconds(expiresIn - 60); // trừ buffer

        return _accessToken;
    }
}