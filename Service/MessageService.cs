using System.Text;
using Newtonsoft.Json;

public class ZaloMessageService
{
    private readonly HttpClient _http;
    private readonly ZaloTokenService _tokenService;

    private const string MESSAGE_URL = "https://openapi.zalo.me/v3.0/oa/message/cs";

    public ZaloMessageService(HttpClient http, ZaloTokenService tokenService)
    {
        _http = http;
        _tokenService = tokenService;
    }

    // ================= TEXT =================

    public async Task SendTextMessageAsync(string userId, string message)
    {
        var token = await _tokenService.GetAccessTokenAsync();

        var payload = new
        {
            recipient = new { user_id = userId },
            message = new { text = message }
        };

        await SendAsync(token, payload, "SendText");
    }

    // ================= FILE =================

    public async Task SendFileAsync(string userId, string fileUrl)
    {
        var token = await _tokenService.GetAccessTokenAsync();

        var payload = new
        {
            recipient = new { user_id = userId },
            message = new
            {
                attachment = new
                {
                    type = "file",
                    payload = new
                    {
                        url = fileUrl
                    }
                }
            }
        };

        await SendAsync(token, payload, "SendFile");
    }

    // ================= CORE =================

    private async Task SendAsync(string token, object payload, string logTag)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, MESSAGE_URL);

        // 🔥 Zalo dùng header này (KHÔNG phải Bearer)
        request.Headers.Add("access_token", token);

        request.Content = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"===== {logTag} RESPONSE =====");
        Console.WriteLine(result);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Zalo API error: {result}");
        }
    }
}