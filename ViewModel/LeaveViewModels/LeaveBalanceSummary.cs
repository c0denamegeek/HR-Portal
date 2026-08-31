namespace HR_Portal.ViewModel.LeaveViewModels
{
    public class LeaveBalanceSummary
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal TotalDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public int Year { get; set; }
        public decimal RemainingDays => TotalDays - UsedDays - PendingDays;
    }
}
