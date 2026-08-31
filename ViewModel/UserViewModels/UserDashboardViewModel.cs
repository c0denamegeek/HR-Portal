using HR_Portal.Models.Domain;
using HR_Portal.ViewModel.LeaveViewModels;

namespace HR_Portal.ViewModel.UserViewModels
{
    // ════════════════════════════════════════════════════════════════
    // USER DASHBOARD
    // ════════════════════════════════════════════════════════════════

    /// <summary>Employee / manager dashboard view model.</summary>
    public class UserDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsManager { get; set; }

        /// <summary>"Your Manager" for employees, "HR Admin" for managers.</summary>
        public string ApproverLabel { get; set; } = string.Empty;

        public IEnumerable<LeaveBalanceSummary> Balances { get; set; } = new List<LeaveBalanceSummary>();
        public IEnumerable<LeaveRequest> RecentRequests { get; set; } = new List<LeaveRequest>();

        /// <summary>Only populated when IsManager = true.</summary>
        public IEnumerable<LeaveRequest> PendingTeamRequests { get; set; } = new List<LeaveRequest>();

        public int PendingTeamCount => PendingTeamRequests.Count();
        public int SelectedYear { get; set; } = DateTime.Today.Year;
    }
}
