using HR_Portal.Data;
using HR_Portal.Interfaces;
using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.AdminViewModels;
using HR_Portal.ViewModel.LeaveViewModels;
using Microsoft.EntityFrameworkCore;

namespace HR_Portal.Services
{

    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _db;

        public LeaveService(AppDbContext db) => _db = db;

        // ── Any User ──────────────────────────────────────────────────────

        public async Task<IEnumerable<LeaveBalanceSummary>> GetLeaveBalancesAsync(string userId, int? year = null)
        {
            var targetYear = year ?? DateTime.Today.Year;
            return await _db.LeaveBalances
                .Where(b => b.EmployeeId == userId && b.Year == targetYear)
                .Include(b => b.LeaveType)
                .Select(b => new LeaveBalanceSummary
                {
                    LeaveTypeId = b.LeaveTypeId,
                    LeaveTypeName = b.LeaveType.Name,
                    TotalDays = b.TotalDays,
                    UsedDays = b.UsedDays,
                    PendingDays = b.PendingDays,
                    CarriedForwardDays = b.CarriedForwardDays,
                    Year = b.Year
                })
                .ToListAsync();
        }

        public async Task<LeaveRequest> SubmitRequestAsync(string userId, LeaveRequestViewModel vm)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found.");

            var totalDays = CalculateWorkingDays(vm.StartDate, vm.EndDate);

            var balance = await _db.LeaveBalances.FirstOrDefaultAsync(b =>
                b.EmployeeId == userId &&
                b.LeaveTypeId == vm.LeaveTypeId &&
                b.Year == vm.StartDate.Year)
                ?? throw new InvalidOperationException("No leave balance found for the selected leave type.");

            if (balance.RemainingDays < totalDays)
                throw new InvalidOperationException(
                    $"Insufficient balance. You have {balance.RemainingDays} day(s) remaining.");

            // Employees → routed to their manager. Managers → null (Admin queue).
            var approverId = user.IsManager ? null : user.ManagerId;

            if (!user.IsManager && string.IsNullOrEmpty(user.ManagerId))
                throw new InvalidOperationException(
                    "You do not have a line manager assigned. Please contact HR before submitting leave.");

            var request = new LeaveRequest
            {
                EmployeeId = userId,
                LeaveTypeId = vm.LeaveTypeId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                TotalDays = totalDays,
                Reason = vm.Reason,
                Status = LeaveStatus.Pending,
                ApproverId = approverId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            balance.PendingDays += totalDays;
            _db.LeaveRequests.Add(request);
            await _db.SaveChangesAsync();
            return request;
        }

        public async Task<IEnumerable<LeaveRequest>> GetMyRequestsAsync(
            string userId, LeaveStatus? status = null, int? year = null)
        {
            var query = _db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.Approver)
                .Where(r => r.EmployeeId == userId);

            if (status.HasValue) query = query.Where(r => r.Status == status.Value);
            if (year.HasValue) query = query.Where(r => r.StartDate.Year == year.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveHistoryAsync(
            string employeeId, int? year = null, LeaveStatus? status = null)
        {
            var query = _db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.Approver)
                .Where(r => r.EmployeeId == employeeId);

            if (year.HasValue) query = query.Where(r => r.StartDate.Year == year.Value);
            if (status.HasValue) query = query.Where(r => r.Status == status.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task CancelRequestAsync(int requestId, string userId)
        {
            var request = await _db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new InvalidOperationException("Leave request not found.");

            if (request.EmployeeId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own requests.");

            if (!request.CanBeCancelled)
                throw new InvalidOperationException("This request cannot be cancelled.");

            var balance = await GetBalanceAsync(userId, request.LeaveTypeId, request.StartDate.Year);

            if (request.Status == LeaveStatus.Pending)
                balance.PendingDays -= request.TotalDays;
            else if (request.Status == LeaveStatus.Approved)
                balance.UsedDays -= request.TotalDays;

            request.Status = LeaveStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ── Manager ───────────────────────────────────────────────────────

        public async Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(string managerId)
        {
            return await _db.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Where(r => r.ApproverId == managerId && r.Status == LeaveStatus.Pending)
                .OrderBy(r => r.StartDate)
                .ToListAsync();
        }

        public async Task ApproveRequestAsync(int requestId, string reviewerId, string? comments)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            // Either the assigned manager or an Admin (ApproverId = null path)
            EnsureCanReview(request, reviewerId);

            if (request.Status != LeaveStatus.Pending)
                throw new InvalidOperationException("Only pending requests can be approved.");

            var balance = await GetBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            balance.PendingDays -= request.TotalDays;
            balance.UsedDays += request.TotalDays;

            request.Status = LeaveStatus.Approved;
            request.ReviewerComments = comments;
            request.ReviewedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task RejectRequestAsync(int requestId, string reviewerId, string? comments)
        {
            var request = await GetRequestOrThrowAsync(requestId);
            EnsureCanReview(request, reviewerId);

            if (request.Status != LeaveStatus.Pending)
                throw new InvalidOperationException("Only pending requests can be rejected.");

            var balance = await GetBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            balance.PendingDays -= request.TotalDays;

            request.Status = LeaveStatus.Rejected;
            request.ReviewerComments = comments;
            request.ReviewedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ── Admin ─────────────────────────────────────────────────────────

        public async Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync(
            string? department = null, LeaveStatus? status = null, int? year = null)
        {
            var query = _db.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.Approver)
                .Include(r => r.LeaveType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(department))
                query = query.Where(r => r.Employee.Department == department);
            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);
            if (year.HasValue)
                query = query.Where(r => r.StartDate.Year == year.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetManagerRequestsPendingAdminAsync()
        {
            return await _db.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Where(r => r.ApproverId == null &&
                            r.Status == LeaveStatus.Pending &&
                            r.Employee.IsManager)
                .OrderBy(r => r.StartDate)
                .ToListAsync();
        }

        public async Task UpsertLeaveBalanceAsync(LeaveBalanceEditViewModel vm)
        {
            var balance = await _db.LeaveBalances.FirstOrDefaultAsync(b =>
                b.EmployeeId == vm.EmployeeId &&
                b.LeaveTypeId == vm.LeaveTypeId &&
                b.Year == vm.Year);

            if (balance is null)
            {
                _db.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = vm.EmployeeId,
                    LeaveTypeId = vm.LeaveTypeId,
                    Year = vm.Year,
                    TotalDays = vm.TotalDays,
                    CarriedForwardDays = vm.CarriedForwardDays
                });
            }
            else
            {
                balance.TotalDays = vm.TotalDays;
                balance.CarriedForwardDays = vm.CarriedForwardDays;
            }

            await _db.SaveChangesAsync();
        }

        public async Task ProvisionYearlyBalancesAsync(int year)
        {
            var employees = await _db.Users.Where(u => u.IsActive).ToListAsync();
            var leaveTypes = await _db.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            var existing = await _db.LeaveBalances.Where(b => b.Year == year).ToListAsync();

            foreach (var emp in employees)
            {
                foreach (var lt in leaveTypes)
                {
                    if (existing.Any(b => b.EmployeeId == emp.Id && b.LeaveTypeId == lt.Id))
                        continue;

                    decimal carried = 0;
                    if (lt.IsCarryForwardAllowed)
                    {
                        var prev = await _db.LeaveBalances.FirstOrDefaultAsync(b =>
                            b.EmployeeId == emp.Id && b.LeaveTypeId == lt.Id && b.Year == year - 1);
                        if (prev is not null)
                            carried = Math.Min(prev.RemainingDays, lt.MaxCarryForwardDays);
                    }

                    _db.LeaveBalances.Add(new LeaveBalance
                    {
                        EmployeeId = emp.Id,
                        LeaveTypeId = lt.Id,
                        Year = year,
                        TotalDays = lt.DefaultDaysPerYear + carried,
                        CarriedForwardDays = carried
                    });
                }
            }

            await _db.SaveChangesAsync();
        }

        // ── Shared ────────────────────────────────────────────────────────

        public int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate) return 0;
            int days = 0;
            for (var d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    days++;
            return days;
        }

        public async Task<LeaveRequest?> GetRequestByIdAsync(int requestId)
            => await _db.LeaveRequests
                    .Include(r => r.Employee)
                    .Include(r => r.Approver)
                    .Include(r => r.LeaveType)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

        // ── Private helpers ───────────────────────────────────────────────

        private async Task<LeaveRequest> GetRequestOrThrowAsync(int requestId)
            => await _db.LeaveRequests.Include(r => r.Employee)
                    .FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new InvalidOperationException("Leave request not found.");

        private async Task<LeaveBalance> GetBalanceAsync(string employeeId, int leaveTypeId, int year)
            => await _db.LeaveBalances.FirstOrDefaultAsync(b =>
                    b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year)
                ?? throw new InvalidOperationException("Leave balance record not found.");

        /// <summary>
        /// A request can be reviewed by:
        ///   - Its assigned ApproverId (the employee's manager), OR
        ///   - Any Admin (when ApproverId is null — manager's own leave).
        /// The caller is responsible for checking the Admin role before calling this.
        /// </summary>
        private static void EnsureCanReview(LeaveRequest request, string reviewerId)
        {
            if (request.ApproverId is not null && request.ApproverId != reviewerId)
                throw new UnauthorizedAccessException(
                    "You are not the assigned approver for this leave request.");
        }
    }
    
}
