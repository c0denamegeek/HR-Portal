using HR_Portal.Constants;
using HR_Portal.Interfaces;
using HR_Portal.Models;
using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.AdminViewModels;
using HR_Portal.ViewModel.LeaveViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Portal.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminDashboardController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly IUserAccountService _userAccountService;
        private readonly UserManager<Users> _userManager;

        public AdminDashboardController(
            ILeaveService leaveService,
            IUserAccountService userAccountService,
            UserManager<Users> userManager)
        {
            _leaveService = leaveService;
            _userAccountService = userAccountService;
            _userManager = userManager;
        }

        // GET /AdminDashboard/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var allRequests = await _leaveService.GetAllRequestsAsync();
            var today = DateTime.Today;

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _leaveService.GetActiveUserCountAsync(),
                TotalPending = allRequests.Count(r => r.Status == LeaveStatus.Pending),
                TotalApproved = allRequests.Count(r => r.Status == LeaveStatus.Approved),
                TotalRejected = allRequests.Count(r => r.Status == LeaveStatus.Rejected),
                EmployeesOnLeaveToday = allRequests.Count(r =>
                    r.Status == LeaveStatus.Approved &&
                    r.StartDate.Date <= today &&
                    r.EndDate.Date >= today),
                RecentRequests = allRequests.Take(10),
                SelectedYear = DateTime.Today.Year
            };

            return View(vm);
        }

        // GET /AdminDashboard/AllRequests
        [HttpGet]
        public async Task<IActionResult> AllRequests(string? department, LeaveStatus? status, int? year)
        {
            var requests = await _leaveService.GetAllRequestsAsync(department, status, year);

            ViewBag.Departments = await _leaveService.GetDepartmentsAsync();
            ViewBag.SelectedDept = department;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedYear = year ?? DateTime.Today.Year;

            return View(requests);
        }

        // GET /AdminDashboard/ReviewRequest/5
        [HttpGet]
        public async Task<IActionResult> ReviewRequest(int id)
        {
            var request = await _leaveService.GetRequestByIdAsync(id);
            if (request is null) return NotFound();

            var balances = await _leaveService.GetLeaveBalancesAsync(request.EmployeeId);
            var remaining = balances
                .FirstOrDefault(b => b.LeaveTypeName == request.LeaveType.Name)?.RemainingDays ?? 0;

            var vm = new LeaveApprovalViewModel
            {
                LeaveRequestId = request.Id,
                EmployeeFullName = request.Employee.FullName,
                Department = request.Employee.Department ?? "-",
                LeaveTypeName = request.LeaveType.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                EmployeeRemainingBalance = remaining
            };

            return View(vm);
        }

        // POST /AdminDashboard/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(LeaveApprovalViewModel vm)
        {
            var adminId = _userManager.GetUserId(User)!;
            try
            {
                await _leaveService.ApproveRequestAsync(vm.LeaveRequestId, adminId, vm.Comments);
                TempData["Success"] = "Leave request approved.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(AllRequests));
        }

        // POST /AdminDashboard/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(LeaveApprovalViewModel vm)
        {
            var adminId = _userManager.GetUserId(User)!;
            try
            {
                await _leaveService.RejectRequestAsync(vm.LeaveRequestId, adminId, vm.Comments);
                TempData["Success"] = "Leave request rejected.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(AllRequests));
        }

        // GET /AdminDashboard/Balances
        [HttpGet]
        public async Task<IActionResult> Balances(string? employeeId)
        {
            ViewBag.Users = await _userAccountService.GetAllUsersAsync();
            ViewBag.SelectedEmployee = employeeId;

            if (string.IsNullOrEmpty(employeeId))
                return View(Enumerable.Empty<LeaveBalanceSummary>());

            var balances = await _leaveService.GetLeaveBalancesAsync(employeeId);
            return View(balances);
        }

        // GET /AdminDashboard/EditBalance
        [HttpGet]
        public async Task<IActionResult> EditBalance(string employeeId, int leaveTypeId, int year)
        {
            var user = await _userAccountService.GetUserByIdAsync(employeeId);
            if (user is null) return NotFound();

            var leaveTypes = await _leaveService.GetLeaveTypesAsync();
            var existing = await _leaveService.GetLeaveBalanceAsync(employeeId, leaveTypeId, year);

            var vm = new LeaveBalanceEditViewModel
            {
                BalanceId = existing?.Id,
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId == 0 ? leaveTypes.FirstOrDefault()?.Id ?? 0 : leaveTypeId,
                Year = year == 0 ? DateTime.Today.Year : year,
                TotalDays = existing?.TotalDays ?? 0,
                CarriedForwardDays = existing?.CarriedForwardDays ?? 0,
                EmployeeFullName = user.FullName,
                AvailableLeaveTypes = leaveTypes
            };

            return View(vm);
        }

        // POST /AdminDashboard/EditBalance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBalance(LeaveBalanceEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableLeaveTypes = await _leaveService.GetLeaveTypesAsync();
                var user = await _userAccountService.GetUserByIdAsync(vm.EmployeeId);
                vm.EmployeeFullName = user?.FullName ?? string.Empty;
                return View(vm);
            }

            await _leaveService.UpsertLeaveBalanceAsync(vm);
            TempData["Success"] = $"Balance updated for {vm.EmployeeFullName}.";
            return RedirectToAction(nameof(Balances), new { employeeId = vm.EmployeeId });
        }

        // POST /AdminDashboard/ProvisionBalances
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProvisionBalances(int year)
        {
            await _leaveService.ProvisionYearlyBalancesAsync(year);
            TempData["Success"] = $"Leave balances provisioned for {year}.";
            return RedirectToAction(nameof(Balances));
        }

        // GET /AdminDashboard/LeaveTypes
        [HttpGet]
        public async Task<IActionResult> LeaveTypes()
        {
            var types = await _leaveService.GetLeaveTypesAsync();
            return View(types);
        }

        // GET /AdminDashboard/LeaveHistory
        [HttpGet]
        public async Task<IActionResult> LeaveHistory(string? search, string? department)
        {
            var users = await _userAccountService.GetAllUsersAsync(search);

            if (!string.IsNullOrEmpty(department))
                users = users.Where(u => u.Department == department);

            ViewBag.Search = search;
            ViewBag.Departments = await _leaveService.GetDepartmentsAsync();
            ViewBag.SelectedDept = department;

            return View(users);
        }

        // GET /AdminDashboard/EmployeeLeaveHistory
        [HttpGet]
        public async Task<IActionResult> EmployeeLeaveHistory(
            string employeeId, int? year, LeaveStatus? status)
        {
            var user = await _userAccountService.GetUserByIdAsync(employeeId);
            if (user is null) return NotFound();

            var requests = await _leaveService.GetEmployeeLeaveHistoryAsync(employeeId, year, status);
            var balances = await _leaveService.GetLeaveBalancesAsync(employeeId, year);

            var vm = new EmployeeLeaveHistoryViewModel
            {
                EmployeeId = employeeId,
                EmployeeFullName = user.FullName,
                Department = user.Department ?? "-",
                JobTitle = user.JobTitle ?? "-",
                Requests = requests,
                Balances = balances,
                SelectedYear = year ?? DateTime.Today.Year,
                StatusFilter = status
            };

            return View(vm);
        }
    }
}
