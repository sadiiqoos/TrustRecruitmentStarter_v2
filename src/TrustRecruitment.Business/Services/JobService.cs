using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;

namespace TrustRecruitment.Business.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task CreateAsync(JobDto job, CancellationToken cancellationToken = default)
    {
        var entity = new Job
        {
            Id = job.Id == Guid.Empty ? Guid.NewGuid() : job.Id,
            Title = job.Title,
            Department = job.Department,
            Location = job.Location,
            Country = job.Country,
            EmploymentType = job.EmploymentType,
            Description = job.Description,
            IsActive = job.IsActive
        };

        await _jobRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<JobDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        => (await _jobRepository.GetActiveAsync(cancellationToken)).Select(Map).ToList();

    public async Task<IReadOnlyList<JobDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _jobRepository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<JobDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        return job is null ? null : Map(job);
    }

    private static JobDto Map(Job job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Department = job.Department,
        Location = job.Location,
        Country = job.Country,
        EmploymentType = job.EmploymentType,
        Description = job.Description,
        IsActive = job.IsActive
    };
}
