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
    private readonly IServiceScopeFactory _scopeFactory;

    public ZaloWebhookController(TestIdOption options, GoogleSheetService sheetService, ReportService reportService, FileStorageService fileService, ZaloMessageService zaloService, IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _sheetService = sheetService;
        _reportService = reportService;
        _fileService = fileService;
        _zaloService = zaloService;
        _scopeFactory = scopeFactory;
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
            Console.WriteLine($"Received webhook from user {userId}");
            // chạy nền
            var message = payload.GetProperty("message").GetProperty("text").GetString();
            if (message != "BAOCAO_GVCN")
            {
                Console.WriteLine($"Received message '{message}' from user {userId}, ignoring...");
                await _zaloService.SendTextMessageAsync(userId, "Hello! To get the report, please send the message 'BAOCAO_GVCN'.");

                return Ok();
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();
                    var fileService = scope.ServiceProvider.GetRequiredService<FileStorageService>();
                    var zaloService = scope.ServiceProvider.GetRequiredService<ZaloMessageService>();

                    Console.WriteLine($"Processing report for {userId}");

                    await zaloService.SendTextMessageAsync(userId, "Đang chuẩn bị báo cáo...");

                    var fileBytes = await reportService.GenerateExcel();
                    var cloudService = scope.ServiceProvider.GetRequiredService<CloudinaryService>();

                    var fileUrl = await cloudService.UploadFileAsync(fileBytes);
                    Console.WriteLine("FILE URL: " + fileUrl);
                    await zaloService.SendTextMessageAsync(userId, fileUrl);

                    //await zaloService.SendFileAsync(userId, fileUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("🔥 TASK ERROR:");
                    Console.WriteLine(ex.ToString());
                }
            });

            return Ok(); // ⚡ trả ngay
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
    private async Task Process(string userId)
    {
        var fileBytes = await _reportService.GenerateExcel();
        var fileUrl = await _fileService.SaveFile(fileBytes);
        await _zaloService.SendFileAsync(userId, fileUrl);
    }
}