public class FileStorageService
{
    public async Task<string> SaveFile(byte[] fileBytes)
    {
        var fileName = $"report_{DateTime.Now.Ticks}.xlsx";
        var path = Path.Combine("wwwroot/files", fileName);

        await File.WriteAllBytesAsync(path, fileBytes);

        return $"https://prjme.onrender.com/files/{fileName}";
    }
}