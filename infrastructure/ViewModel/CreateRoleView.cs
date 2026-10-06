using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.ViewModel
{
    public class CreateRoleView
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [Display(Name = "Role Name")]
        public string? Name { get; set; }
    }
}
