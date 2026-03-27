using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/zalo")]
public class ZaloAuthController : ControllerBase
{
    private readonly ZaloTokenService _tokenService;

    public ZaloAuthController(ZaloTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing code");

        await _tokenService.ExchangeCodeFromCallbackAsync(code);

        return Ok("Zalo connected successfully ✅");
    }
}