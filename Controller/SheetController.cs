using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/sheet")]
public class SheetController : ControllerBase
{
    private readonly GoogleSheetService _service = new();

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _service.GetData();
        return Ok(data);
    }
}