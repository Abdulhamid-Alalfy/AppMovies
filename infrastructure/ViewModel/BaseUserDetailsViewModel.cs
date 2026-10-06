using System.ComponentModel.DataAnnotations;

namespace infrastructure.ViewModel
{
    public abstract class BaseUserDetailsViewModel
    {
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureURL { get; set; }
    }
}
