using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

public class GoogleSheetService
{
    private readonly SheetsService _service;
    private readonly string _spreadsheetId = "1t-URSVO9nb1pPtPr_V2RJqYFhKSpjYIvzZdO1gBmSCk";

    public GoogleSheetService()
    {
        var credential = GoogleCredential
            .FromFile("credentials.json")
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

        return response.Values;
    }
}