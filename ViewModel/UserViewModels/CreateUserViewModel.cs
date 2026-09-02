using HR_Portal.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.UserViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Display(Name = "Clock Number")]
        public int? ClockNumber { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Temporary Password")]
        public string TemporaryPassword { get; set; }

        // ── Leave system additions ─────────────────────────────────
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [Display(Name = "Is Manager")]
        public bool IsManager { get; set; } = false;

        [Display(Name = "Line Manager")]
        public string? ManagerId { get; set; }

        // Populated by controller
        public IEnumerable<Users> AvailableManagers { get; set; } = new List<Users>();
    }
}