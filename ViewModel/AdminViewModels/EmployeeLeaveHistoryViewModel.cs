using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.LeaveViewModels;

namespace HR_Portal.ViewModel.AdminViewModels
{
    public class EmployeeLeaveHistoryViewModel
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public IEnumerable<LeaveRequest> Requests { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveBalanceSummary> Balances { get; set; } = new List<LeaveBalanceSummary>();
        public int SelectedYear { get; set; } = DateTime.Today.Year;
        public LeaveStatus? StatusFilter { get; set; }

        public int TotalTaken => Requests
            .Where(r => r.Status == LeaveStatus.Approved).Sum(r => r.TotalDays);
        public int TotalPending => Requests
            .Where(r => r.Status == LeaveStatus.Pending).Sum(r => r.TotalDays);
    }
}
