using TrustRecruitment.Business.DTOs;

namespace TrustRecruitment.Business.Interfaces;

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationDto>> GetByCandidateAsync(string candidateUserId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateApplicationDto dto, CancellationToken cancellationToken = default);
}
