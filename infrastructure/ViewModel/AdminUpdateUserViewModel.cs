using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace infrastructure.ViewModel
{
    public class AdminUpdateUserViewModel : BaseUserEditViewModel
    {
        [Display(Name = "Roles")]
        [ValidateNever]
        public List<SelectRoleViewModel> Roles { get; set; } = new List<SelectRoleViewModel>();
    }

    public class SelectRoleViewModel
    {
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Selected")]
        public bool IsSelected { get; set; }
    }
}
