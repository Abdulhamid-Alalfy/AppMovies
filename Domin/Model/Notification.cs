using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domin.Model
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public string? UserId { get; set; } // إذا كانت الإشعار لمستخدم معين

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // إذا كان الإشعار لجميع المستخدمين
        public bool IsForAllUsers { get; set; } = false;

    }


    public enum NotificationType
    {
        NewMovie = 1,
        SystemAlert = 2,
        MovieUpdate = 3,
        SpecialOffer = 4

    }
}
