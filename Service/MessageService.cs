using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ZaloMessageService
{
    private readonly HttpClient _http;
    private readonly ZaloTokenService _tokenService;

    // 🔥 tách riêng 2 endpoint
    private const string TEXT_URL = "https://openapi.zalo.me/v3.0/oa/message/cs";
    private const string FILE_URL = "https://openapi.zalo.me/v3.0/oa/message";

    public ZaloMessageService(HttpClient http, ZaloTokenService tokenService)
    {
        _http = http;
        _tokenService = tokenService;
    }

    // ================= TEXT =================

    public async Task SendTextMessageAsync(string userId, string message)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId is required");

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("message is required");

        var token = await _tokenService.GetAccessTokenAsync();

        var payload = new
        {
            recipient = new { user_id = userId },
            message = new { text = message }
        };

        await SendAsync(token, payload, "SendText", TEXT_URL);
    }

    // ================= FILE =================

    public async Task SendFileAsync(string userId, string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId is required");

        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("fileUrl is required");

        var token = await _tokenService.GetAccessTokenAsync();

        Console.WriteLine("FILE URL: " + fileUrl); // debug

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

        await SendAsync(token, payload, "SendFile", FILE_URL);
    }

    // ================= CORE =================

    private async Task SendAsync(string token, object payload, string logTag, string url)
    {
        if (string.IsNullOrEmpty(token))
            throw new Exception("Access token is null");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        // 🔥 header chuẩn của Zalo
        request.Headers.Remove("access_token");
        request.Headers.TryAddWithoutValidation("access_token", token);

        request.Content = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _http.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"===== {logTag} RESPONSE =====");
        Console.WriteLine(result);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Zalo API error: {result}");
        }
    }
}