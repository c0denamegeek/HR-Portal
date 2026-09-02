using HR_Portal.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HR_Portal.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Users (self-referencing manager) ─────────────────────────
            builder.Entity<Users>(e =>
            {
                e.HasOne(u => u.Manager)
                 .WithMany(u => u.DirectReports)
                 .HasForeignKey(u => u.ManagerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LeaveRequest ──────────────────────────────────────────────
            builder.Entity<LeaveRequest>(e =>
            {
                e.HasOne(r => r.Employee)
                 .WithMany(u => u.LeaveRequests)
                 .HasForeignKey(r => r.EmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Approver)
                 .WithMany(u => u.ReviewedRequests)
                 .HasForeignKey(r => r.ApproverId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.LeaveType)
                 .WithMany(t => t.LeaveRequests)
                 .HasForeignKey(r => r.LeaveTypeId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(r => r.Status).HasConversion<int>();

                e.HasIndex(r => r.EmployeeId);
                e.HasIndex(r => r.ApproverId);
                e.HasIndex(r => r.Status);
            });

            // ── LeaveBalance ──────────────────────────────────────────────
            builder.Entity<LeaveBalance>(e =>
            {
                // One row per employee / leave type / year
                e.HasIndex(b => new { b.EmployeeId, b.LeaveTypeId, b.Year })
                 .IsUnique();

                e.HasOne(b => b.Employee)
                 .WithMany(u => u.LeaveBalances)
                 .HasForeignKey(b => b.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(b => b.LeaveType)
                 .WithMany(t => t.LeaveBalances)
                 .HasForeignKey(b => b.LeaveTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Seed roles ────────────────────────────────────────────────
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "6e71959f-f22f-47c1-a189-d666bd1fe831",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "1ccb1664-5b62-4473-807e-a50ee0c44ef5"
                },
                new IdentityRole
                {
                    Id = "754d806c-c5ac-4e00-a422-5bb35005d358",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "cc07f136-fa95-4787-bf6b-d2cbe9b58a06"
                }
            );

            // ── Seed leave types ──────────────────────────────────────────
            builder.Entity<LeaveType>().HasData(
                new LeaveType { Id = 1, Name = "Annual Leave", DefaultDaysPerYear = 21, IsCarryForwardAllowed = true, MaxCarryForwardDays = 5, IsActive = true },
                new LeaveType { Id = 2, Name = "Sick Leave", DefaultDaysPerYear = 10, RequiresDocumentation = true, IsActive = true },
                new LeaveType { Id = 3, Name = "Family Responsibility Leave", DefaultDaysPerYear = 3, IsActive = true },
                new LeaveType { Id = 4, Name = "Maternity Leave", DefaultDaysPerYear = 120, RequiresDocumentation = true, MinimumNoticeDays = 30, IsActive = true },
                new LeaveType { Id = 5, Name = "Study Leave", DefaultDaysPerYear = 5, RequiresDocumentation = true, MinimumNoticeDays = 14, IsActive = true }
            );
        }
    }
}
