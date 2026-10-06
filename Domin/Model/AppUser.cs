using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Domin.Model
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureURL { get; set; }
        public List<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public List<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
    }
}
