using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domin.Model
{
    public class UserFavorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MovieId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
        [ForeignKey("MovieId")]
        public Movie? Movies { get; set; }
    }
}
