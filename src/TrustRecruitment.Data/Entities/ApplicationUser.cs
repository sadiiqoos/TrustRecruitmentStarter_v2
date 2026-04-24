using MongoDB.Bson.Serialization.Attributes;

namespace TrustRecruitment.Data.Entities;

public class ApplicationUser
{
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "User";
}