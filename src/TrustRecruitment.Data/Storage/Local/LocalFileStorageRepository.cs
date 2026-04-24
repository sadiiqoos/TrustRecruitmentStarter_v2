using TrustRecruitment.Data.Storage.Interfaces;

namespace TrustRecruitment.Data.Storage.Local;

public class LocalFileStorageRepository : IFileStorageRepository
{
    private readonly string _rootPath;

    public LocalFileStorageRepository(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);

        var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_rootPath, safeFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return fullPath;
    }
}
