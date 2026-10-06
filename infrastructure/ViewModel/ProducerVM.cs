using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace infrastructure.ViewModel
{
    public class ProducerVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Producer name is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Biography is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Biography must be between 10 and 2000 characters")]
        public string Bio { get; set; } = string.Empty;

        public string? Image { get; set; }
        public IFormFile? ImagePath { get; set; }
    }

    public class ProducerListVM
    {
        public  int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? Image { get; set; }
        public IFormFile? ImagePath { get; set; }
    }
    public class EditProducerVM 
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Producer name is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Biography is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Biography must be between 10 and 2000 characters")]
        public string Bio { get; set; } = string.Empty;

        public string? Image { get; set; }
        public IFormFile? ImagePath { get; set; }
    }
}
