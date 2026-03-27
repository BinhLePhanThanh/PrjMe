using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TestIdOption _options;
    private readonly ZaloMessageService _messageService;

    public TestController(TestIdOption options, ZaloMessageService messageService)
    {
        _options = options;
        _messageService = messageService;
    }

    [HttpGet("id")]
    public IActionResult GetId()
    {
        return Ok(_options.userId);
    }
    [HttpPost("id")]
    public async Task<IActionResult> sendMessage()
    {
        if (string.IsNullOrEmpty(_options.userId))
        {
            return BadRequest("UserId chưa có");
        }

        // Gửi tin nhắn
        await _messageService.SendTextMessageAsync(_options.userId, "Hello from TestController!");

        return Ok();
    }

}
