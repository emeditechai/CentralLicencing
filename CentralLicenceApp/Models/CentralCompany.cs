namespace CentralLicenceApp.Models
{
    /// <summary>Row of Central_Company. Connection strings are never loaded; only whether one is set.</summary>
    public class CentralCompany
    {
        public int ID { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int ApplicationCompanyID { get; set; }
        public string InstanceKey { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ApiUrl { get; set; }
        public bool HasConnectionString { get; set; }
        public int UserCount { get; set; }
        public DateTime? LastUserAddedDate { get; set; }

        public string MaskedInstanceKey => CentralMask.Mask(InstanceKey);
    }

    /// <summary>Row of Central_Users.</summary>
    public class CentralUser
    {
        public long ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public int? ApplicationUserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string InstanceKey { get; set; } = string.Empty;

        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }

        public string MaskedInstanceKey => CentralMask.Mask(InstanceKey);
    }

    public static class CentralMask
    {
        // Shows enough to tell instances apart without exposing the full key.
        public static string Mask(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "—";
            return value.Length <= 8 ? new string('•', value.Length) : value[..6] + "••••" + value[^2..];
        }
    }
}
