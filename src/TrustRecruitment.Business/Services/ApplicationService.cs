using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IFileStorageRepository fileStorageRepository,
        ILogger<ApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _fileStorageRepository = fileStorageRepository;
        _logger = logger;
    }

    // Används av ApplicationsController (med filuppladdning)
    public async Task<Guid> CreateAsync(CreateApplicationDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating application with CV upload. JobId={JobId}, CandidateUserId={CandidateUserId}",
            dto.JobId, dto.CandidateUserId);

        var job = await _jobRepository.GetByIdAsync(dto.JobId, cancellationToken);
        if (job is null || !job.IsActive)
        {
            _logger.LogWarning("Application rejected — job not found or inactive. JobId={JobId}", dto.JobId);
            throw new InvalidOperationException("The selected job does not exist or is not active.");
        }

        var alreadyApplied = await _applicationRepository.ExistsAsync(dto.CandidateUserId, dto.JobId, cancellationToken);
        if (alreadyApplied)
        {
            _logger.LogWarning("Duplicate application attempt. JobId={JobId}, CandidateUserId={CandidateUserId}",
                dto.JobId, dto.CandidateUserId);
            throw new InvalidOperationException("This candidate has already applied for the selected job.");
        }

        // Loggar INTE kandidatens e-post/namn i detalj — bara CV-filnamnet
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

        _logger.LogInformation("Application created. ApplicationId={ApplicationId}, JobId={JobId}",
            application.Id, application.JobId);

        return application.Id;
    }

    // Används av JobsController (utan filuppladdning)
    public async Task<Guid> CreateAsync(ApplicationDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating application without CV. JobId={JobId}", dto.JobId);

        var job = await _jobRepository.GetByIdAsync(dto.JobId, cancellationToken);
        if (job is null || !job.IsActive)
        {
            _logger.LogWarning("Application rejected — job not found or inactive. JobId={JobId}", dto.JobId);
            throw new InvalidOperationException("The selected job does not exist or is not active.");
        }

        var application = new JobApplication
        {
            JobId = dto.JobId,
            CandidateUserId = dto.CandidateEmail.ToLowerInvariant(),
            CandidateName = dto.CandidateName,
            CandidateEmail = dto.CandidateEmail,
            Country = dto.Country,
            CvFileName = dto.CvFileName,
            CvStoragePath = string.Empty,
            AppliedUtc = DateTime.UtcNow
        };

        await _applicationRepository.AddAsync(application, cancellationToken);

        _logger.LogInformation("Application created. ApplicationId={ApplicationId}, JobId={JobId}",
            application.Id, application.JobId);

        return application.Id;
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _applicationRepository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<IReadOnlyList<ApplicationDto>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        => (await _applicationRepository.GetAllAsync(cancellationToken))
            .Where(a => a.JobId == jobId)
            .Select(Map)
            .ToList();

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
