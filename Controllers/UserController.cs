using HR_Portal.Constants;
using HR_Portal.Interfaces;
using HR_Portal.Models.Enums;
using HR_Portal.ViewModel.AdminViewModels;
using HR_Portal.ViewModel.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Portal.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : Controller
    {
        private readonly IUserAccountService _userAccountService;

        public UsersController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        // GET /Users/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userAccountService.GetAllUsersAsync();

            // Attach roles to ViewBag so the view can display them
            var userRoles = new Dictionary<string, string>();
            foreach (var u in users)
                userRoles[u.Id] = u.IsManager ? "Manager" : "User";

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET /Users/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await BuildVmAsync();
            return View(vm);
        }

        // POST /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserManagementViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(await BuildVmAsync(vm));

            var result = await _userAccountService.CreateUserAsync(
                name: vm.Name,
                surname: vm.Surname,
                email: vm.Email,
                department: vm.Department.ToString(),
                password: vm.TemporaryPassword!,
                role: vm.Role,
                clockNumber: vm.ClockNumber,
                jobTitle: vm.JobTitle,
                isManager: vm.IsManager,
                managerId: vm.ManagerId);

            if (result.Succeeded)
            {
                TempData["Success"] = $"{vm.Name} {vm.Surname} created successfully.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(await BuildVmAsync(vm));
        }

        // GET /Users/EditUser/id
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userAccountService.GetUserByIdAsync(id);
            if (user is null) return NotFound();

            var vm = new UserManagementViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Surname = user.Surname ?? string.Empty,
                Email = user.Email!,
                Department = Enum.TryParse<Department>(user.Department, out var dept)
                 ? dept
                 : Department.Operations,
                JobTitle = user.JobTitle,
                ClockNumber = user.ClockNumber,
                IsManager = user.IsManager,
                ManagerId = user.ManagerId,
                Role = user.IsManager ? "Manager" : "User"
            };

            return View(await BuildVmAsync(vm));
        }

        // POST /Users/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserManagementViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(await BuildVmAsync(vm));

            var result = await _userAccountService.UpdateUserAsync(vm);

            if (result.Succeeded)
            {
                TempData["Success"] = $"{vm.Name} {vm.Surname} updated successfully.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(await BuildVmAsync(vm));
        }

        // POST /Users/Deactivate/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _userAccountService.DeactivateUserAsync(id);

            if (result.Succeeded)
                TempData["Success"] = "User deactivated successfully.";
            else
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Users));
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private async Task<UserManagementViewModel> BuildVmAsync(UserManagementViewModel? vm = null)
        {
            var allUsers = await _userAccountService.GetAllUsersAsync();
            var managers = allUsers.Where(u => u.IsManager).ToList();

            var result = vm ?? new UserManagementViewModel();
            result.AvailableManagers = managers;
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string? search)
        {
            var users = await _userAccountService.GetAllUsersAsync(search);

            var userRoles = new Dictionary<string, string>();
            foreach (var u in users)
                userRoles[u.Id] = u.IsManager ? "Manager" : "User";

            ViewBag.UserRoles = userRoles;
            ViewBag.Search = search;
            return View(users);
        }
    }
}