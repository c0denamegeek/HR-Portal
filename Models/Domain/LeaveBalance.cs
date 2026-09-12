using HR_Portal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Portal.Models.Domain
{
    public class LeaveBalance
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "Total Days")]
        public decimal TotalDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "Used Days")]
        public decimal UsedDays { get; set; } = 0;

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "Pending Days")]
        public decimal PendingDays { get; set; } = 0;

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "Carried Forward")]
        public decimal CarriedForwardDays { get; set; } = 0;

        // ── Computed ──────────────────────────────────────────────────────

        [NotMapped]
        public decimal RemainingDays => TotalDays - UsedDays - PendingDays;

        // ── Navigation ────────────────────────────────────────────────────

        [ForeignKey(nameof(EmployeeId))]
        public virtual Users Employee { get; set; } = null!;

        [ForeignKey(nameof(LeaveTypeId))]
        public virtual LeaveType LeaveType { get; set; } = null!;
    }
}