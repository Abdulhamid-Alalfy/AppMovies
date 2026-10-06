using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domin.Model
{
    public class UserNotificationRead
    {
        public int Id { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("NotificationId")]
        public virtual Notification? Notification { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }
}
