namespace CentralLicenceApp.Models
{
    public class FinancialYearMaster
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FYCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsCurrentFY { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
