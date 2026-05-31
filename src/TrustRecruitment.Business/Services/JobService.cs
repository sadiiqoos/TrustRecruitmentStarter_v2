using Microsoft.Extensions.Logging;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;

namespace TrustRecruitment.Business.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobService> _logger;

    public JobService(IJobRepository jobRepository, ILogger<JobService> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task CreateAsync(JobDto job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating job. Title={Title}, Department={Department}", job.Title, job.Department);

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

        _logger.LogInformation("Job created successfully. JobId={JobId}, Title={Title}", entity.Id, entity.Title);
    }

    public async Task UpdateAsync(Guid id, JobDto job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating job. JobId={JobId}", id);

        var entity = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Update failed — job not found. JobId={JobId}", id);
            throw new InvalidOperationException("Job not found.");
        }

        entity.Title = job.Title;
        entity.Department = job.Department;
        entity.Location = job.Location;
        entity.Country = job.Country;
        entity.EmploymentType = job.EmploymentType;
        entity.Description = job.Description;
        entity.IsActive = job.IsActive;

        await _jobRepository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Job updated successfully. JobId={JobId}, Title={Title}", id, entity.Title);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting job. JobId={JobId}", id);
        await _jobRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Job deleted. JobId={JobId}", id);
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
