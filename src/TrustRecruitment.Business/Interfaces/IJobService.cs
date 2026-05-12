// IJobService.cs
using TrustRecruitment.Business.DTOs;

namespace TrustRecruitment.Business.Interfaces;

public interface IJobService
{
    Task<IReadOnlyList<JobDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<JobDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(JobDto job, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, JobDto job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}