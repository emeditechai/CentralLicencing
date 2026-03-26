using System.ComponentModel.DataAnnotations;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public bool ShowRoleSelectionPopup { get; set; }
        public string? PendingSelectionToken { get; set; }
        public List<RoleMaster> AvailableRoles { get; set; } = new();
    }
}
