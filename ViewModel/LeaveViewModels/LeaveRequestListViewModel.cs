using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;

namespace HR_Portal.ViewModel.LeaveViewModels
{
    /// <summary>History + filter view for a user's own requests.</summary>
    public class LeaveRequestListViewModel
    {
        public IEnumerable<LeaveRequest> Requests { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveBalanceSummary> Balances { get; set; } = new List<LeaveBalanceSummary>();
        public int SelectedYear { get; set; } = DateTime.Today.Year;
        public LeaveStatus? StatusFilter { get; set; }
    }
}
