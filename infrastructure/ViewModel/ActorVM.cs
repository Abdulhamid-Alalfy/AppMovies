using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace infrastructure.ViewModel
{
     public  class ActorVM
    {
        public string Name { get; set; }

        public string Bio { get; set; }

        public string? Image { get; set; }

        public IFormFile? ImagePath { get; set; }

    }

    public class EditVM {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string? Image { get; set; }
        public IFormFile? ImagePath { get; set; }
    }

    public class ActorListVm 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string? Image { get; set; }
        public IFormFile? ImagePath { get; set; }

    }
}
