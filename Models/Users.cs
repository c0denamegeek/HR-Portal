using HR_Portal.Models.Domain;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Portal.Models
{
    public class Users : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public int? ClockNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public bool IsManager { get; set; } = false;
        public string? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
        [ForeignKey(nameof(ManagerId))]
        public virtual Users? Manager { get; set; }
        public virtual ICollection<Users> DirectReports { get; set; } = new List<Users>();

        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveRequest> ReviewedRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

        [NotMapped]
        public string FullName => $"{Name} {Surname}";


    }
}
