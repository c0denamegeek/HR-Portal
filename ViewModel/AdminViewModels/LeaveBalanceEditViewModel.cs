using HR_Portal.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.AdminViewModels
{
    /// <summary>Admin form to adjust a specific leave balance.</summary>
    public class LeaveBalanceEditViewModel
    {
        public int? BalanceId { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.Today.Year;

        [Required]
        [Range(0, 365)]
        [Display(Name = "Total Days Entitlement")]
        public decimal TotalDays { get; set; }

        [Range(0, 365)]
        [Display(Name = "Carried Forward Days")]
        public decimal CarriedForwardDays { get; set; } = 0;

        // Populated by controller
        public string EmployeeFullName { get; set; } = string.Empty;
        public IEnumerable<LeaveType> AvailableLeaveTypes { get; set; } = new List<LeaveType>();
    }
}