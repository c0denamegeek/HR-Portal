using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.AdminViewModels;
using HR_Portal.ViewModel.LeaveViewModels;

namespace HR_Portal.Interfaces
{
    public interface ILeaveService
    {
        // ── Any User ──────────────────────────────────────────────────────

        Task<IEnumerable<LeaveBalanceSummary>> GetLeaveBalancesAsync(string userId, int? year = null);

        Task<LeaveRequest> SubmitRequestAsync(string userId, LeaveRequestViewModel vm);

        Task<IEnumerable<LeaveRequest>> GetMyRequestsAsync(string userId, LeaveStatus? status = null, int? year = null);

        Task CancelRequestAsync(int requestId, string userId);

        // ── Manager ───────────────────────────────────────────────────────

        Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(string managerId);

        Task ApproveRequestAsync(int requestId, string reviewerId, string? comments);

        Task RejectRequestAsync(int requestId, string reviewerId, string? comments);

        // ── Admin ─────────────────────────────────────────────────────────

        Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync(string? department = null, LeaveStatus? status = null, int? year = null);

        Task<IEnumerable<LeaveRequest>> GetManagerRequestsPendingAdminAsync();

        Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveHistoryAsync(string employeeId, int? year = null, LeaveStatus? status = null);

        Task UpsertLeaveBalanceAsync(LeaveBalanceEditViewModel vm);

        Task ProvisionYearlyBalancesAsync(int year);

        // ── Admin helpers ─────────────────────────────────────────────────

        Task<int> GetActiveUserCountAsync();

        Task<IEnumerable<string?>> GetDepartmentsAsync();

        Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(bool activeOnly = true);

        Task<LeaveBalance?> GetLeaveBalanceAsync(string employeeId, int leaveTypeId, int year);

        Task<AdminDashboardViewModel> GetDashboardStatsAsync();

        // ── Shared ────────────────────────────────────────────────────────

        int CalculateWorkingDays(DateTime startDate, DateTime endDate);

        Task<LeaveRequest?> GetRequestByIdAsync(int requestId);
    }
}