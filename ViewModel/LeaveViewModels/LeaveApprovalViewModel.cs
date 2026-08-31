using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.LeaveViewModels
{
    /// <summary>
    /// Shown to a manager when they open a pending request from a direct report.
    /// </summary>
    public class LeaveApprovalViewModel
    {
        public int LeaveRequestId { get; set; }

        [StringLength(1000)]
        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        // Read-only context
        public string EmployeeFullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal EmployeeRemainingBalance { get; set; }
    }
}
