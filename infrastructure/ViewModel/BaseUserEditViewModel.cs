using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace infrastructure.ViewModel
{
    public abstract class BaseUserEditViewModel
    {
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }

        public string? PhotoPath { get; set; }
    }
}
