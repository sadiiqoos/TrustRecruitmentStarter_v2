using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using TrustRecruitment.Data.Storage.Interfaces;

namespace TrustRecruitment.Data.Storage.Blob;

/// <summary>
/// Laddar upp filer till Azure Blob Storage med Managed Identity (ingen connection string).
/// </summary>
public class BlobFileStorageRepository : IFileStorageRepository
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<BlobFileStorageRepository> _logger;

    public BlobFileStorageRepository(BlobServiceClient blobServiceClient, ILogger<BlobFileStorageRepository> logger)
    {
        _container = blobServiceClient.GetBlobContainerClient("cv-uploads");
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var blobClient = _container.GetBlobClient(blobName);

        _logger.LogInformation("Uploading CV to blob storage. FileName={FileName}", fileName);

        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        _logger.LogInformation("CV uploaded successfully. BlobName={BlobName}, Uri={Uri}", blobName, blobClient.Uri);

        return blobClient.Uri.ToString();
    }
}
