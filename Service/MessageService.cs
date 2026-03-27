using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

public class ZaloMessageService
{
    private readonly HttpClient _http;
    private readonly ZaloTokenService _tokenService;

    public ZaloMessageService(HttpClient http, ZaloTokenService tokenService)
    {
        _http = http;
        _tokenService = tokenService;
    }

    public async Task SendTextMessageAsync(string userId, string message)
    {
        var token = await _tokenService.GetAccessTokenAsync();

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            recipient = new { user_id = userId },
            message = new { text = message }
        };

        var json = JsonConvert.SerializeObject(body);

        var response = await _http.PostAsync(
            "https://openapi.zalo.me/v3.0/oa/message/cs",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Zalo API error: {result}");
        }
    }
}