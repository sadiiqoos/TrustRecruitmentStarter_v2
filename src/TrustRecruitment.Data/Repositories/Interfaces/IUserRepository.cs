using TrustRecruitment.Data.Entities;

namespace TrustRecruitment.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}