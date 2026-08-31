using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.AdminViewModels;
using HR_Portal.ViewModel.LeaveViewModels;

namespace HR_Portal.Interfaces
{
    public interface ILeaveService
    {
        // ── Any User (employee or manager) ────────────────────────────────

        Task<IEnumerable<LeaveBalanceSummary>> GetLeaveBalancesAsync(string userId, int? year = null);


        /// Submits a leave request.
        /// - Employee: ApproverId set to their Manager's Id.
        /// - Manager:  ApproverId left null (Admin handles the queue).
        Task<LeaveRequest> SubmitRequestAsync(string userId, LeaveRequestViewModel vm);

        Task<IEnumerable<LeaveRequest>> GetMyRequestsAsync(string userId, LeaveStatus? status = null, int? year = null);

        Task CancelRequestAsync(int requestId, string userId);

        // ── Manager approval (IsManager = true, still User role) ──────────

        /// Pending requests from this manager's direct reports.
        Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(string managerId);

        Task ApproveRequestAsync(int requestId, string reviewerId, string? comments);

        Task RejectRequestAsync(int requestId, string reviewerId, string? comments);

        /// All requests across the company.
        /// Includes manager-submitted requests (ApproverId = null) that only Admin can action.

        Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync(string? department = null, LeaveStatus? status = null, int? year = null);

        /// <summary>Pending requests submitted by managers (no ApproverId — Admin queue).</summary>
        Task<IEnumerable<LeaveRequest>> GetManagerRequestsPendingAdminAsync();

        Task UpsertLeaveBalanceAsync(LeaveBalanceEditViewModel vm);

        Task ProvisionYearlyBalancesAsync(int year);

        // ── Shared ────────────────────────────────────────────────────────

        int CalculateWorkingDays(DateTime startDate, DateTime endDate);

        Task<LeaveRequest?> GetRequestByIdAsync(int requestId);
    }
}
