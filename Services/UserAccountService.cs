using HR_Portal.Interfaces;
using HR_Portal.Models;
using HR_Portal.ViewModel.AdminViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HR_Portal.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(UserManager<Users> userManager, ILogger<UserAccountService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IdentityResult> CreateUserAsync(
            string name,
            string surname,
            string email,
            string department,
            string password,
            string role,
            int? clockNumber = null,
            string? jobTitle = null,
            bool isManager = false,
            string? managerId = null)
        {
            var user = new Users
            {
                Name = name,
                Surname = surname,
                ClockNumber = clockNumber,
                JobTitle = jobTitle,
                Department = department,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                IsManager = isManager,
                ManagerId = role == "Admin" ? null : managerId,
                IsActive = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user {Email}: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return result;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to {Email}: {Errors}",
                    role, email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return result;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _userManager.Users
                .Include(u => u.Manager)
                .Where(u => u.IsActive)
                .OrderBy(u => u.Surname)
                .ToListAsync();
        }

        public async Task<Users?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IdentityResult> UpdateUserAsync(UserManagementViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.Id!);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.Name = vm.Name;
            user.Surname = vm.Surname;
            user.Department = vm.Department.ToString();
            user.JobTitle = vm.JobTitle;
            user.ClockNumber = vm.ClockNumber;
            user.IsManager = vm.IsManager;
            user.ManagerId = vm.Role == "Admin" ? null : vm.ManagerId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return result;

            // Update role
            var existingRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, existingRoles);
            await _userManager.AddToRoleAsync(user, vm.Role);

            return result;
        }

        public async Task<IdentityResult> DeactivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.IsActive = false;
            return await _userManager.UpdateAsync(user);
        }
    }
}