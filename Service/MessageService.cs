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
    public async Task SendFile(string userId, string fileUrl)
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
                    payload = new { url = fileUrl }
                }
            }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://openapi.zalo.me/v3.0/oa/message"
        );

        request.Headers.Add("access_token", token);
        request.Content = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("SendFile response:");
        Console.WriteLine(result);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Zalo send file error: {result}");
        }
    }
}