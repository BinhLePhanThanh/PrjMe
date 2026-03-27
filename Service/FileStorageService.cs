public class FileStorageService
{
    public async Task<string> SaveFile(byte[] fileBytes)
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

        Directory.CreateDirectory(folder);

        var fileName = $"report_{DateTime.UtcNow.Ticks}.xlsx";
        var fullPath = Path.Combine(folder, fileName);

        Console.WriteLine("SAVE PATH: " + fullPath); // 🔥 debug

        await File.WriteAllBytesAsync(fullPath, fileBytes);

        return $"https://prjme.onrender.com/files/{fileName}";
    }
}