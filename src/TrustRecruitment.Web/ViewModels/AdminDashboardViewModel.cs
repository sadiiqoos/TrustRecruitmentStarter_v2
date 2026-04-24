using TrustRecruitment.Business.DTOs;

namespace TrustRecruitment.Web.ViewModels;

public class AdminDashboardViewModel
{
    public IReadOnlyList<JobDto> Jobs { get; set; } = [];
    public IReadOnlyList<ApplicationDto> Applications { get; set; } = [];
}
