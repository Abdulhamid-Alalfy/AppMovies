using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domin.Model
{
    public class WatchHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int MovieId { get; set; }

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        [ForeignKey("MovieId")]
        public Movie? Movie { get; set; }

        public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public int Progress { get; set; } = 0;

        public TimeSpan? WatchedDuration { get; set; }
    }
}
