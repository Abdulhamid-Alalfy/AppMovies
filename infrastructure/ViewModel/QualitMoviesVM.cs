using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace infrastructure.ViewModel
{
    public class QualitMoviesVM
    {

        public class ManageMovieQualitiesViewModel
        {
            public int MovieId { get; set; }
            public string MovieTitle { get; set; }
            public string MoviePoster { get; set; }
            public List<MovieQualityViewModel> ExistingQualities { get; set; } = new List<MovieQualityViewModel>();
        }

        public class CreateMovieQualityViewModel
        {
            public int MovieId { get; set; }
            public string MovieTitle { get; set; }

            [Required(ErrorMessage = "نوع الجودة مطلوب")]
            [Display(Name = "نوع الجودة")]
            public string QualityType { get; set; }

            [Required(ErrorMessage = "ملف الفيديو مطلوب")]
            [Display(Name = "ملف الفيديو")]
            public IFormFile VideoFile { get; set; }

            [Display(Name = "الكودك")]
            public string Codec { get; set; } = "H.264";

            [Display(Name = "معدل البت")]
            public string Bitrate { get; set; } = "2000 kbps";

            [Display(Name = "الحجم التقريبي (MB)")]
            [Range(1, 10000, ErrorMessage = "الحجم يجب أن يكون بين 1 و 10000 ميجابايت")]
            public double FileSizeMB { get; set; } = 500;
        }

        public class QualityUploadItem
        {
            public string QualityType { get; set; }
            public IFormFile? VideoFile { get; set; }
        }

        public class CreateMultipleMovieQualitiesViewModel
        {
            public int MovieId { get; set; }
            public string MovieTitle { get; set; }
            public List<string> ExistingQualityTypes { get; set; } = new List<string>();

            [Display(Name = "الكودك")]
            public string Codec { get; set; } = "H.264";

            [Display(Name = "معدل البت")]
            public string Bitrate { get; set; } = "2000 kbps";

            public List<QualityUploadItem> Items { get; set; } = new List<QualityUploadItem>();
        }

        public class MovieQualityViewModel
        {
            public int Id { get; set; }
            public int MovieId { get; set; }
            public string Quality { get; set; }
            public string VideoUrl { get; set; }
            public string FileSize { get; set; }
            public int ViewCount { get; set; }
            public string Codec { get; set; }
            public string Bitrate { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsAvailable { get; set; }
        }

        public class QualityOptionViewModel
        {
            public int QualityId { get; set; }
            public string QualityType { get; set; } // 360p, 480p, etc.
            public string DisplayName { get; set; }
            public string VideoUrl { get; set; }
            public string FileSizeFormatted { get; set; } // "1.2 GB"
            public string Codec { get; set; }
            public int Bitrate { get; set; }
            public string BadgeClass { get; set; }
            public bool IsRecommended { get; set; }
            public int ViewCount { get; set; }
        }

        public class MoviePlayerViewModel
        {
            public int MovieId { get; set; }
            public string Title { get; set; }
            public string CurrentVideoUrl { get; set; }
            public string CurrentQuality { get; set; }
            public List<QualityOptionViewModel> AvailableQualities { get; set; }
            public string PosterUrl { get; set; }
            public int Duration { get; set; }
        }

        public class EditMovieQualityViewModel
        {
            public int Id { get; set; }
            public int MovieId { get; set; }
            public string MovieTitle { get; set; }

            [Required(ErrorMessage = "نوع الجودة مطلوب")]
            [Display(Name = "نوع الجودة")]
            public string QualityType { get; set; }

            [Display(Name = "ملف الفيديو")]
            public IFormFile? VideoFile { get; set; }

            public string ExistingVideoUrl { get; set; }

            [Display(Name = "الكودك")]
            public string Codec { get; set; } = "H.264";

            [Display(Name = "معدل البت")]
            public string Bitrate { get; set; } = "2000 kbps";

            [Display(Name = "الحجم التقريبي (MB)")]
            [Range(1, 10000, ErrorMessage = "الحجم يجب أن يكون بين 1 و 10000 ميجابايت")]
            public double FileSizeMB { get; set; } = 500;

            [Display(Name = "الحالة")]
            public bool IsAvailable { get; set; } = true;
        }
    }
}
