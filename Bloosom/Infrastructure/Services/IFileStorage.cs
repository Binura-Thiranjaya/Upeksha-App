namespace Bloosom.Infrastructure.Services;

public interface IFileStorage
{
    Task<string> SaveFileAsync(Stream stream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string publicUrl);
}

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly string _publicBase;

    public LocalFileStorage(IConfiguration config)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _publicBase = "/uploads";
        if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string contentType)
    {
        var safeName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
        var full = Path.Combine(_basePath, safeName);
        using var fs = File.Create(full);
        await stream.CopyToAsync(fs);
        return _publicBase + "/" + safeName;
    }

    public Task<bool> DeleteFileAsync(string publicUrl)
    {
        try
        {
            var name = Path.GetFileName(publicUrl);
            var full = Path.Combine(_basePath, name);
            if (File.Exists(full)) File.Delete(full);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}

