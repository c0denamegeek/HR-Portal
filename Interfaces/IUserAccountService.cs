using HR_Portal.Models.Domain;
using HR_Portal.ViewModel.AdminViewModels;
using Microsoft.AspNetCore.Identity;

namespace HR_Portal.Interfaces
{
    public interface IUserAccountService
    {
        Task<IdentityResult> CreateUserAsync(
            string name,
            string surname,
            string email,
            string department,
            string password,
            string role,
            int? clockNumber = null,
            string? jobTitle = null,
            bool isManager = false,
            string? managerId = null);

        Task<IEnumerable<Users>> GetAllUsersAsync();

        Task<Users?> GetUserByIdAsync(string id);

        Task<IdentityResult> UpdateUserAsync(UserManagementViewModel vm);

        Task<IdentityResult> DeactivateUserAsync(string id);

        Task<IEnumerable<Users>> GetAllUsersAsync(string? search = null);
    }
}
