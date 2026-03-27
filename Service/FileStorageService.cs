public class FileStorageService
{
    public async Task<string> SaveFile(byte[] fileBytes)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "wwwroot", "files");

        // 🔥 tạo folder nếu chưa có
        Directory.CreateDirectory(folder);

        var fileName = $"report_{DateTime.UtcNow.Ticks}.xlsx";
        var fullPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(fullPath, fileBytes);

        return $"https://prjme.onrender.com/files/{fileName}";
    }
}