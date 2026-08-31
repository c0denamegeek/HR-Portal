using HR_Portal.Models;
using HR_Portal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.AdminViewModels
{
    public class UserManagementViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string Surname { get; set; } = string.Empty;

        [Display(Name = "Clock Number")]
        public int? ClockNumber { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public Department Department { get; set; }   // ← enum now

        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [Display(Name = "Is Manager")]
        public bool IsManager { get; set; } = false;

        [Display(Name = "Line Manager")]
        public string? ManagerId { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Temporary Password")]
        public string? TemporaryPassword { get; set; }

        // Populated by controller
        public IEnumerable<Users> AvailableManagers { get; set; } = new List<Users>();

        // Computed
        public bool IsEdit => Id is not null;
    }
}
