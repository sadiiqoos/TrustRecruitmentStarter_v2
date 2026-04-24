namespace TrustRecruitment.Business.DTOs;

public class CreateApplicationDto
{
    public Guid JobId { get; set; }
    public string CandidateUserId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CvFileName { get; set; } = string.Empty;
    public Stream CvContent { get; set; } = Stream.Null;
}
