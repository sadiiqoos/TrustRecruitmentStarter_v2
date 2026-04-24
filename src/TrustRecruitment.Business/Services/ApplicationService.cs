using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Storage.Interfaces;

namespace TrustRecruitment.Business.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IFileStorageRepository _fileStorageRepository;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IFileStorageRepository fileStorageRepository)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _fileStorageRepository = fileStorageRepository;
    }

    public async Task<Guid> CreateAsync(CreateApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(dto.JobId, cancellationToken);
        if (job is null || !job.IsActive)
        {
            throw new InvalidOperationException("The selected job does not exist or is not active.");
        }

        var alreadyApplied = await _applicationRepository.ExistsAsync(dto.CandidateUserId, dto.JobId, cancellationToken);
        if (alreadyApplied)
        {
            throw new InvalidOperationException("This candidate has already applied for the selected job.");
        }

        var storagePath = await _fileStorageRepository.SaveAsync(dto.CvContent, dto.CvFileName, cancellationToken);

        var application = new JobApplication
        {
            JobId = dto.JobId,
            CandidateUserId = dto.CandidateUserId,
            CandidateName = dto.CandidateName,
            CandidateEmail = dto.CandidateEmail,
            Country = dto.Country,
            CvFileName = dto.CvFileName,
            CvStoragePath = storagePath,
            AppliedUtc = DateTime.UtcNow
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        return application.Id;
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _applicationRepository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<IReadOnlyList<ApplicationDto>> GetByCandidateAsync(string candidateUserId, CancellationToken cancellationToken = default)
        => (await _applicationRepository.GetByCandidateAsync(candidateUserId, cancellationToken)).Select(Map).ToList();

    private static ApplicationDto Map(JobApplication application) => new()
    {
        Id = application.Id,
        JobId = application.JobId,
        CandidateName = application.CandidateName,
        CandidateEmail = application.CandidateEmail,
        Country = application.Country,
        CvFileName = application.CvFileName,
        AppliedUtc = application.AppliedUtc
    };
}
