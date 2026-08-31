using System.ComponentModel.DataAnnotations;


namespace HR_Portal.Models.Domain
{
    public class LeaveType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 365)]
        [Display(Name = "Default Days Per Year")]
        public int DefaultDaysPerYear { get; set; }

        [Display(Name = "Allow Carry Forward")]
        public bool IsCarryForwardAllowed { get; set; } = false;

        [Range(0, 365)]
        [Display(Name = "Max Carry Forward Days")]
        public int MaxCarryForwardDays { get; set; } = 0;

        [Display(Name = "Requires Documentation")]
        public bool RequiresDocumentation { get; set; } = false;

        [Display(Name = "Minimum Notice (Days)")]
        public int MinimumNoticeDays { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────

        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    }
}
