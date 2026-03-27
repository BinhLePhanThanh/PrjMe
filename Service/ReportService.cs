public class ReportService
{
    private readonly GoogleSheetService _sheetService;

    public ReportService(GoogleSheetService sheetService)
    {
        _sheetService = sheetService;
    }

    public async Task<byte[]> GenerateExcel()
    {
        var data = await _sheetService.GetData();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Report");

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].Count; j++)
            {
                ws.Cell(i + 1, j + 1).Value = data[i][j]?.ToString();
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}