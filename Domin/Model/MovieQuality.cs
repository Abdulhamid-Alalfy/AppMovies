using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domin.Model
{
    public class MovieQuality
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        [StringLength(50)]
        public string QualityType { get; set; } = string.Empty; // 360p, 480p, 720p, 1080p, 4K

        [Required]
        [StringLength(500)]
        public string VideoUrl { get; set; } = string.Empty;

        public long FileSize { get; set; } // بالبايت

        [StringLength(10)]
        public string? Codec { get; set; } // H.264, H.265, VP9

        public int Bitrate { get; set; } // كيلوبت بالثانية

        public bool IsAvailable { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("MovieId")]
        public virtual Movie? Movie { get; set; }
    }
}
