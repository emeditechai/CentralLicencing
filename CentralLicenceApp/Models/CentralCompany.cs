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
        public string? WebUrl { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool HasConnectionString { get; set; }
        public int UserCount { get; set; }
        public DateTime? LastUserAddedDate { get; set; }

        public string MaskedInstanceKey => CentralMask.Mask(InstanceKey);

        /// <summary>Validity from ValidFrom / ValidTo: text and badge colour.</summary>
        public (string Text, string Background, string Color) Validity
        {
            get
            {
                var today = DateTime.Today;
                if (ValidFrom == null && ValidTo == null) return ("Not set", "#f1f5f9", "#64748b");
                if (ValidFrom > today) return ("Not yet valid", "#e0f2fe", "#0369a1");
                if (ValidTo < today) return ("Expired", "#fee2e2", "#b91c1c");
                if (ValidTo <= today.AddDays(30)) return ($"Expires in {(ValidTo.Value - today).Days} day(s)", "#fef3c7", "#92400e");
                return ("Active", "#dcfce7", "#166534");
            }
        }

        /// <summary>WebUrl as a safe link target, or null when it isn't a web address.</summary>
        public string? WebLink
        {
            get
            {
                var raw = WebUrl?.Trim() ?? "";
                if (raw.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) raw = "https://" + raw;
                return Uri.TryCreate(raw, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps) ? raw : null;
            }
        }
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
