using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/zalo")]
public class ZaloAuthController : ControllerBase
{
    private readonly ZaloTokenService _tokenService;
    private readonly ZaloOptions _options;

    public ZaloAuthController(
        ZaloTokenService tokenService,
        IOptions<ZaloOptions> options)
    {
        _tokenService = tokenService;
        _options = options.Value;
    }

    // 🔥 STEP 1: Redirect admin OA sang Zalo
    [HttpGet("login")]
    public IActionResult Login()
    {
        var url = "https://oauth.zaloapp.com/v4/oa/permission?" +
                  $"app_id={_options.AppId}" +
                  $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}";

        return Redirect(url);
    }

    // 🔥 STEP 2: Callback nhận code
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing code");

        await _tokenService.ExchangeCodeAsync(code);

        return Ok("OA connected successfully ✅");
    }
}