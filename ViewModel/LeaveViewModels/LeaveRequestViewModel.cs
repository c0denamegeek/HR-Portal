using HR_Portal.Models;
using HR_Portal.Models.Domain;
using HR_Portal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.LeaveViewModels
{
    public class LeaveRequestViewModel
    {
        [Required]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;

        // ── Populated by controller ───────────────────────────────
        public IEnumerable<LeaveType> AvailableLeaveTypes { get; set; } = new List<LeaveType>();
        public IEnumerable<LeaveBalanceSummary> CurrentBalances { get; set; } = new List<LeaveBalanceSummary>();

        /// <summary>
        /// Name shown on the form so the user knows who will receive their request.
        /// "Your Manager" for employees, "HR Admin" for managers.
        /// </summary>
        public string ApproverLabel { get; set; } = string.Empty;
    }
}

