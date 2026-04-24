namespace TrustRecruitment.Business.DTOs;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CvFileName { get; set; } = string.Empty;
    public DateTime AppliedUtc { get; set; }
}
