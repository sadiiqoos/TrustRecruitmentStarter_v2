using TrustRecruitment.Data.Entities;

namespace TrustRecruitment.Data.Repositories.Interfaces;

public interface IApplicationRepository
{
    Task<IReadOnlyList<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobApplication>> GetByCandidateAsync(string candidateUserId, CancellationToken cancellationToken = default);
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string candidateUserId, Guid jobId, CancellationToken cancellationToken = default);
    Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);
}
