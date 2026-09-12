using HR_Portal.Models;
using HR_Portal.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Portal.Models.Domain
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Days")]
        public int TotalDays { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        /// <summary>
        /// The manager assigned to approve this request.
        /// Null when the requester is a manager — Admin approves those.
        /// </summary>
        public string? ApproverId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Reviewed At")]
        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        [Display(Name = "Reviewer Comments")]
        public string? ReviewerComments { get; set; }

        [StringLength(500)]
        [Display(Name = "Supporting Document")]
        public string? DocumentPath { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Submitted At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────

        [ForeignKey(nameof(EmployeeId))]
        public virtual Users Employee { get; set; } = null!;

        [ForeignKey(nameof(ApproverId))]
        public virtual Users? Approver { get; set; }

        [ForeignKey(nameof(LeaveTypeId))]
        public virtual LeaveType LeaveType { get; set; } = null!;

        // ── Computed ──────────────────────────────────────────────────────

        [NotMapped]
        public bool IsPending => Status == LeaveStatus.Pending;

        [NotMapped]
        public bool CanBeCancelled =>
            Status == LeaveStatus.Pending ||
            (Status == LeaveStatus.Approved && StartDate > DateTime.Today);
    }
}