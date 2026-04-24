using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Settings;

namespace TrustRecruitment.Data.Repositories.MongoDb;

public class MongoJobRepository : IJobRepository
{
    private readonly IMongoCollection<Job> _jobs;

    public MongoJobRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings)
    {
        _jobs = database.GetCollection<Job>(settings.Value.JobsCollectionName);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _jobs
            .Find(_ => true)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _jobs
            .Find(job => job.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _jobs
            .Find(job => job.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        await _jobs.InsertOneAsync(job, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        await _jobs.ReplaceOneAsync(
            existingJob => existingJob.Id == job.Id,
            job,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _jobs.DeleteOneAsync(
            job => job.Id == id,
            cancellationToken);
    }
}