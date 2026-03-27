using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/zalo/webhook")]
public class ZaloWebhookController : ControllerBase
{
    private TestIdOption _options;
    private GoogleSheetService _sheetService;

    private readonly ReportService _reportService;
    private readonly FileStorageService _fileService;
    private readonly ZaloMessageService _zaloService;

    public ZaloWebhookController(TestIdOption options, GoogleSheetService sheetService, ReportService reportService, FileStorageService fileService, ZaloMessageService zaloService)
    {
        _options = options;
        _sheetService = sheetService;
        _reportService = reportService;
        _fileService = fileService;
        _zaloService = zaloService;
    }
    [HttpGet]
    public IActionResult Verify()
    {
        return Ok("OK");
    }
    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] JsonElement payload)
    {
        try
        {
            var userId = payload
                .GetProperty("sender")
                .GetProperty("id")
                .GetString();

            var fileBytes = await _reportService.GenerateExcel();

            // 2. Upload → lấy URL
            var fileUrl = await _fileService.SaveFile(fileBytes);

            // 3. Gửi qua Zalo
            await _zaloService.SendFile(userId, fileUrl);

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