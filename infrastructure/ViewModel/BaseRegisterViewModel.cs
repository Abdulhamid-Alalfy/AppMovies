using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace infrastructure.ViewModel
{
    public abstract class BaseRegisterViewModel
    {
        [Required(ErrorMessage = "Username must be Entered")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email must be Entered")]
        [EmailAddress(ErrorMessage = "Email must be entered correctly.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password must be Entered")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password must match")]
        [Required(ErrorMessage = "You must re-enter the password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ProfilePictureURL { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
