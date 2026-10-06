using System.ComponentModel.DataAnnotations;

namespace infrastructure.ViewModel
{
    public class AdminRegisterUserViewModel : BaseRegisterViewModel
    {
        [Required(ErrorMessage = "At least one role must be selected")]
        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Available Roles")]
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
