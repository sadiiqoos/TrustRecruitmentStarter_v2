using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Settings;

namespace TrustRecruitment.Data.Repositories.MongoDb;

public class MongoApplicationRepository : IApplicationRepository
{
    private readonly IMongoCollection<JobApplication> _applications;

    public MongoApplicationRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings)
    {
        _applications = database.GetCollection<JobApplication>(settings.Value.ApplicationsCollectionName);
    }

    public async Task<IReadOnlyList<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _applications
            .Find(_ => true)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobApplication>> GetByCandidateAsync(
        string candidateUserId,
        CancellationToken cancellationToken = default)
    {
        return await _applications
            .Find(application => application.CandidateUserId == candidateUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _applications
            .Find(application => application.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string candidateUserId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var count = await _applications.CountDocumentsAsync(
            application => application.CandidateUserId == candidateUserId && application.JobId == jobId,
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        await _applications.InsertOneAsync(application, cancellationToken: cancellationToken);
    }
}