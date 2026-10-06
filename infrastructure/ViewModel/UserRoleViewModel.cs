using System.ComponentModel.DataAnnotations;

namespace infrastructure.ViewModel
{
    public class UserRoleViewModel
    {
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureURL { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public bool IsBlocked { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}
