namespace TrustRecruitment.Data.Storage.Interfaces;

public interface IFileStorageRepository
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
