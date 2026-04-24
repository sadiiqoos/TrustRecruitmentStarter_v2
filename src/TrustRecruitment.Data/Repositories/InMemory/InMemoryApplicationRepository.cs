using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;

namespace TrustRecruitment.Data.Repositories.InMemory;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly List<JobApplication> _applications = [];

    public Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        _applications.Add(application);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string candidateUserId, Guid jobId, CancellationToken cancellationToken = default)
        => Task.FromResult(_applications.Any(x => x.CandidateUserId == candidateUserId && x.JobId == jobId));

    public Task<IReadOnlyList<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<JobApplication>>(_applications.OrderByDescending(x => x.AppliedUtc).ToList());

    public Task<IReadOnlyList<JobApplication>> GetByCandidateAsync(string candidateUserId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<JobApplication>>(_applications.Where(x => x.CandidateUserId == candidateUserId).OrderByDescending(x => x.AppliedUtc).ToList());

    public Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_applications.FirstOrDefault(x => x.Id == id));
}
