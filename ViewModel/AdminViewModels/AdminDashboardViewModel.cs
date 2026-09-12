using HR_Portal.Models;
using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;

namespace HR_Portal.ViewModel.AdminViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPending { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int EmployeesOnLeaveToday { get; set; }
        public int TotalUsers { get; set; }
        public IEnumerable<LeaveRequest> RecentRequests { get; set; } = new List<LeaveRequest>();

        // Filters
        public string? DepartmentFilter { get; set; }
        public LeaveStatus? StatusFilter { get; set; }
        public int SelectedYear { get; set; } = DateTime.Today.Year;
    }
}