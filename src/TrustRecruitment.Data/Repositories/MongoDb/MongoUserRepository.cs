using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Settings;

namespace TrustRecruitment.Data.Repositories.MongoDb;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<ApplicationUser> _users;

    public MongoUserRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings)
    {
        _users = database.GetCollection<ApplicationUser>(settings.Value.UsersCollectionName);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email = NormalizeEmail(email);

        return await _users
            .Find(user => user.Email == email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _users
            .Find(user => user.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email = NormalizeEmail(email);

        var count = await _users.CountDocumentsAsync(
            user => user.Email == email,
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.Email = NormalizeEmail(user.Email);

        await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}