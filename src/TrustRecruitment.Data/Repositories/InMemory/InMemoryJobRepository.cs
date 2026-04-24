using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;

namespace TrustRecruitment.Data.Repositories.InMemory;

public class InMemoryJobRepository : IJobRepository
{
    private readonly List<Job> _jobs =
    [
        new()
        {
            Title = "Junior .NET Developer",
            Department = "Engineering",
            Location = "Stockholm",
            Country = "Sweden",
            EmploymentType = "Full Time",
            Description = "Starter role for the TrustRecruitment platform."
        },
        new()
        {
            Title = "QA Engineer",
            Department = "Quality",
            Location = "Remote",
            Country = "Sweden",
            EmploymentType = "Contract",
            Description = "Test the recruitment workflows and APIs."
        }
    ];

    public Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        _jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = _jobs.FirstOrDefault(x => x.Id == id);
        if (existing is not null)
        {
            _jobs.Remove(existing);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Job>> GetActiveAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Job>>(_jobs.Where(x => x.IsActive).OrderByDescending(x => x.CreatedUtc).ToList());

    public Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Job>>(_jobs.OrderByDescending(x => x.CreatedUtc).ToList());

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_jobs.FirstOrDefault(x => x.Id == id));

    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        var existing = _jobs.FirstOrDefault(x => x.Id == job.Id);
        if (existing is null)
        {
            return Task.CompletedTask;
        }

        existing.Title = job.Title;
        existing.Department = job.Department;
        existing.Location = job.Location;
        existing.Country = job.Country;
        existing.EmploymentType = job.EmploymentType;
        existing.Description = job.Description;
        existing.IsActive = job.IsActive;

        return Task.CompletedTask;
    }
}
