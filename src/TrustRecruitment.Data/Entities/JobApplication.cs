using MongoDB.Bson.Serialization.Attributes;

namespace TrustRecruitment.Data.Entities;

public class JobApplication
{
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid JobId { get; set; }

    public string CandidateUserId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CvFileName { get; set; } = string.Empty;
    public string CvStoragePath { get; set; } = string.Empty;
    public DateTime AppliedUtc { get; set; } = DateTime.UtcNow;
}