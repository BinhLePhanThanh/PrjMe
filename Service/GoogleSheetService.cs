using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

public class GoogleSheetService
{
    private readonly SheetsService _service;
    private readonly string _spreadsheetId = "1t-URSVO9nb1pPtPr_V2RJqYFhKSpjYIvzZdO1gBmSCk";

    public GoogleSheetService()
    {
        var json = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS");

        if (string.IsNullOrEmpty(json))
            throw new Exception("GOOGLE_CREDENTIALS is missing");

        // 🔥 QUAN TRỌNG: fix newline khi dùng ENV
        json = json.Replace("\\n", "\n");

        var credential = GoogleCredential
            .FromJson(json)
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "SheetReader"
        });
    }

    public async Task<IList<IList<object>>> GetData()
    {
        string range = "'BAOCAO_GVCN'";

        var request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        return response.Values ?? new List<IList<object>>();
    }
}