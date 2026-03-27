using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    public IActionResult Receive([FromBody] JsonElement payload)
    {
        try
        {
            var userId = payload
                .GetProperty("sender")
                .GetProperty("id")
                .GetString();

            Console.WriteLine($"UserId: {userId}");

            _options.userId = userId;

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Ok();
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