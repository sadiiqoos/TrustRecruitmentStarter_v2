namespace TrustRecruitment.Business.Models;

public class AccountResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
}