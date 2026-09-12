using HR_Portal.Constants;
using HR_Portal.Interfaces;
using HR_Portal.Models;
using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.LeaveViewModels;
using HR_Portal.ViewModel.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Portal.Controllers
{
    [Authorize(Roles = Roles.User)]
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly UserManager<Users> _userManager;

        public LeaveController(
            ILeaveService leaveService,
            UserManager<Users> userManager)
        {
            _leaveService = leaveService;
            _userManager = userManager;
        }

        // GET /Leave/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var balances = await _leaveService.GetLeaveBalancesAsync(user.Id);
            var requests = await _leaveService.GetMyRequestsAsync(user.Id);

            // Only call once and reuse
            var pendingTeam = user.IsManager
                ? await _leaveService.GetPendingForManagerAsync(user.Id)
                : Enumerable.Empty<LeaveRequest>();

            ViewBag.IsManager = user.IsManager;
            ViewBag.PendingTeamCount = pendingTeam.Count();

            var vm = new UserDashboardViewModel
            {
                FullName = user.FullName,
                IsManager = user.IsManager,
                ApproverLabel = user.IsManager ? "HR Admin" : "Your Manager",
                Balances = balances,
                RecentRequests = requests.Take(5),
                PendingTeamRequests = pendingTeam,
                SelectedYear = DateTime.Today.Year
            };

            return View(vm);
        }

        // GET /Leave/Apply
        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            if (!user.IsManager && string.IsNullOrEmpty(user.ManagerId))
            {
                TempData["Warning"] = "You do not have a line manager assigned. Please contact HR before applying for leave.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View(await BuildApplyVmAsync(user));
        }

        // POST /Leave/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveRequestViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            if (vm.EndDate < vm.StartDate)
                ModelState.AddModelError(nameof(vm.EndDate), "End date must be on or after the start date.");

            if (vm.StartDate.Date < DateTime.Today)
                ModelState.AddModelError(nameof(vm.StartDate), "Start date cannot be in the past.");

            if (!ModelState.IsValid)
                return View(await BuildApplyVmAsync(user, vm));

            try
            {
                await _leaveService.SubmitRequestAsync(user.Id, vm);
                TempData["Success"] = "Your leave request has been submitted successfully.";
                return RedirectToAction(nameof(MyRequests));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(await BuildApplyVmAsync(user, vm));
            }
        }

        // GET /Leave/MyRequests
        [HttpGet]
        public async Task<IActionResult> MyRequests(LeaveStatus? status, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var pendingTeamCount = user.IsManager
                ? (await _leaveService.GetPendingForManagerAsync(user.Id)).Count()
                : 0;

            ViewBag.IsManager = user.IsManager;
            ViewBag.PendingTeamCount = pendingTeamCount;

            var vm = new LeaveRequestListViewModel
            {
                Requests = await _leaveService.GetMyRequestsAsync(user.Id, status, year),
                Balances = await _leaveService.GetLeaveBalancesAsync(user.Id, year),
                SelectedYear = year ?? DateTime.Today.Year,
                StatusFilter = status
            };

            return View(vm);
        }

        // GET /Leave/TeamRequests (managers only)
        [HttpGet]
        public async Task<IActionResult> TeamRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();
            if (!user.IsManager) return Forbid();

            ViewBag.IsManager = true;
            ViewBag.PendingTeamCount = 0;

            var pending = await _leaveService.GetPendingForManagerAsync(user.Id);
            return View(pending);
        }

        // GET /Leave/ReviewRequest/5 (managers only)
        [HttpGet]
        public async Task<IActionResult> ReviewRequest(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();
            if (!user.IsManager) return Forbid();

            var request = await _leaveService.GetRequestByIdAsync(id);
            if (request is null) return NotFound();
            if (request.ApproverId != user.Id) return Forbid();

            var balances = await _leaveService.GetLeaveBalancesAsync(request.EmployeeId);
            var remaining = balances
                .FirstOrDefault(b => b.LeaveTypeName == request.LeaveType.Name)?.RemainingDays ?? 0;

            ViewBag.IsManager = true;
            ViewBag.PendingTeamCount = 0;

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

        // POST /Leave/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(LeaveApprovalViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();
            if (!user.IsManager) return Forbid();

            try
            {
                await _leaveService.ApproveRequestAsync(vm.LeaveRequestId, user.Id, vm.Comments);
                TempData["Success"] = "Leave request approved.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(TeamRequests));
        }

        // POST /Leave/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(LeaveApprovalViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();
            if (!user.IsManager) return Forbid();

            try
            {
                await _leaveService.RejectRequestAsync(vm.LeaveRequestId, user.Id, vm.Comments);
                TempData["Success"] = "Leave request rejected.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(TeamRequests));
        }

        // POST /Leave/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            try
            {
                await _leaveService.CancelRequestAsync(id, user.Id);
                TempData["Success"] = "Leave request cancelled.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyRequests));
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private async Task<LeaveRequestViewModel> BuildApplyVmAsync(
            Users user, LeaveRequestViewModel? existing = null)
        {
            Users? manager = null;
            if (!string.IsNullOrEmpty(user.ManagerId))
                manager = await _userManager.FindByIdAsync(user.ManagerId);

            var vm = existing ?? new LeaveRequestViewModel();
            vm.AvailableLeaveTypes = await _leaveService.GetLeaveTypesAsync();
            vm.CurrentBalances = await _leaveService.GetLeaveBalancesAsync(user.Id);
            vm.ApproverLabel = user.IsManager
                ? "HR Admin"
                : manager is not null ? manager.FullName : "Not assigned";

            return vm;
        }
    }
}