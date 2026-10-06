using System.ComponentModel.DataAnnotations;

namespace infrastructure.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "You must enter an email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must enter the password")]
        [MinLength(6, ErrorMessage = "The number of characters must not be less than 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remind me?")]
        public bool RememberMe { get; set; }
    }
}
