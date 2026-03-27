using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/zalo/webhook")]
public class ZaloWebhookController : ControllerBase
{
    private TestIdOption _options;

    public ZaloWebhookController(TestIdOption options)
    {
        _options = options;
    }
    [HttpGet]
    public IActionResult Verify()
    {
        return Ok("OK");
    }
    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] dynamic payload)
    {
        try
        {
            Console.WriteLine(payload?.ToString());

            string? userId = payload?.sender?.id;

            if (!string.IsNullOrEmpty(userId))
            {
                _options.userId = userId;
                Console.WriteLine($"UserId: {userId}");
            }
            else
            {
                Console.WriteLine("No sender.id");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            return Ok(); // 🔥 QUAN TRỌNG: vẫn trả 200 cho Zalo
        }
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