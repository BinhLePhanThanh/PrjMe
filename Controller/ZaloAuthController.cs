using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/zalo")]
public class ZaloAuthController : ControllerBase
{
    private readonly ZaloTokenService _tokenService;
    private readonly ZaloOptions _options;

    public ZaloAuthController(ZaloTokenService tokenService, IOptions<ZaloOptions> options)
    {
        _tokenService = tokenService;
        _options = options.Value;
    }

    // 🔥 STEP 1: Redirect user sang Zalo
    [HttpGet("login")]
    public IActionResult Login()
    {
        var codeVerifier = PkceHelper.GenerateCodeVerifier();
        var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);

        // 🔥 encode verifier vào state
        var state = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(codeVerifier));

        var url = "https://oauth.zaloapp.com/v4/permission?" +
                  $"app_id={_options.AppId}" +
                  $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                  $"&code_challenge={codeChallenge}" +
                  $"&code_challenge_method=S256" +
                  $"&state={Uri.EscapeDataString(state)}";

        return Redirect(url);
    }

    // 🔥 STEP 2: Callback
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
      [FromQuery] string code,
      [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing code");

        if (string.IsNullOrEmpty(state))
            return BadRequest("Missing state");

        // 🔥 decode lại code_verifier
        var codeVerifier = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(state));

        await _tokenService.ExchangeCodeFromCallbackAsync(code, codeVerifier);

        return Ok("Zalo connected successfully ✅");
    }
}