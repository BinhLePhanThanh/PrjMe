using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/zalo/webhook")]
public class ZaloWebhookController : ControllerBase
{
    private readonly TestIdOption _options;

    public ZaloWebhookController(TestIdOption options)
    {
        _options = options;
    }
    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] dynamic payload)
    {
        string userId = payload.sender.id;

        Console.WriteLine($"UserId: {userId}");

        // TODO: lưu DB hoặc xử lý
        _options.userId = userId;
        return Ok();
    }
    [HttpGet("/")]
    public ContentResult Home()
    {
        var html = @"
    <html>
      <head>
        <meta name='zalo-platform-site-verification' content='QDs_5eAZBWfxZhbHdh0uIckskNA7m6miE3Wv' />
      </head>
      <body>OK</body>
    </html>";

        return Content(html, "text/html");
    }
}