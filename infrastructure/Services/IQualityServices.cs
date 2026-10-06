using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace infrastructure.Services
{
    public interface IQualityServices
    {
        Task<bool> AddMovieQualityAsync(CreateMovieQualityViewModel model);
        Task<(int Added, int Skipped)> AddMultipleMovieQualitiesAsync(CreateMultipleMovieQualitiesViewModel model);
        Task<bool> DeleteMovieQualityAsync(int qualityId);
        Task<bool> UpdateQualityViewCountAsync(int qualityId);
        Task<bool> UpdateMovieQualityAsync(EditMovieQualityViewModel model);

        Task<EditMovieQualityViewModel> GetQualityForEditAsync(int id); // ✅ الجديدة
        Task<MovieQualityViewModel> GetQualityByIdAsync(int id); // ✅
        Task<MoviePlayerViewModel> GetMoviePlayerDataAsync(int movieId, string quality = null);

        Task<List<QualityOptionViewModel>> GetAvailableQualitiesAsync(int movieId);
        Task<List<MovieQualityViewModel>> GetMovieQualitiesAsync(int movieId);
        Task<MovieDetailsViewModel> GetMovieDetailsByIdAsync(int id);
    }

    public class QualityServices: IQualityServices
    {

        private readonly DB _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ImageFileServices _imageFileServices;
        public QualityServices(DB context, IWebHostEnvironment environment, ImageFileServices imageFileServices)
        {
            _context = context;
            _environment = environment;
            _imageFileServices = imageFileServices;
        }



        public async Task<bool> AddMovieQualityAsync(CreateMovieQualityViewModel model)
        {
            try
            {
                var existingQuality = await _context.MovieQualities
                    .FirstOrDefaultAsync(q => q.MovieId == model.MovieId
                                           && q.QualityType == model.QualityType);

                if (existingQuality != null)
                    return false; // الجودة موجودة بالفعل

                string videoUrl = await VideoServices.UploadVideoAsync(model.VideoFile, _environment.WebRootPath);

                var movieQuality = new MovieQuality
                {
                    MovieId = model.MovieId,
                    QualityType = model.QualityType,
                    VideoUrl = videoUrl,
                    FileSize = model.VideoFile.Length,
                    Codec = model.Codec ?? "H.264",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MovieQualities.Add(movieQuality);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(int Added, int Skipped)> AddMultipleMovieQualitiesAsync(CreateMultipleMovieQualitiesViewModel model)
        {
            int added = 0;
            int skipped = 0;

            var items = (model.Items ?? new List<QualityUploadItem>())
                .Where(i => !string.IsNullOrWhiteSpace(i.QualityType) && i.VideoFile != null && i.VideoFile.Length > 0)
                .GroupBy(i => i.QualityType)
                .Select(g => g.First())
                .ToList();

            if (!items.Any())
                return (0, 0);

            var existingTypes = await _context.MovieQualities
                .Where(q => q.MovieId == model.MovieId)
                .Select(q => q.QualityType)
                .ToListAsync();

            var existingSet = existingTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newItems = items.Where(i =>
            {
                if (existingSet.Contains(i.QualityType))
                {
                    skipped++;
                    return false;
                }

                return true;
            }).ToList();

            if (!newItems.Any())
                return (0, skipped);

            foreach (var item in newItems)
            {
                string videoUrl = await VideoServices.UploadVideoAsync(item.VideoFile!, _environment.WebRootPath);

                _context.MovieQualities.Add(new MovieQuality
                {
                    MovieId = model.MovieId,
                    QualityType = item.QualityType,
                    VideoUrl = videoUrl,
                    FileSize = item.VideoFile!.Length,
                    Codec = model.Codec ?? "H.264",
                    Bitrate = ParseBitrate(model.Bitrate),
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });
                added++;
            }

            if (added > 0)
                await _context.SaveChangesAsync();

            return (added, skipped);
        }

        private static int ParseBitrate(string bitrate)
        {
            if (string.IsNullOrWhiteSpace(bitrate)) return 0;
            var digits = new string(bitrate.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var value) ? value : 0;
        }


        public async Task<bool> UpdateQualityViewCountAsync(int qualityId)
        {
            try
            {
                var quality = await _context.MovieQualities.FindAsync(qualityId);
                if (quality == null) return false;

                quality.ViewCount++;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }



        public async Task<bool> DeleteMovieQualityAsync(int qualityId)
        {
            try
            {
                var quality = await _context.MovieQualities.FindAsync(qualityId);
                if (quality == null) return false;

                var oldVideoUrl = quality.VideoUrl;
                _context.MovieQualities.Remove(quality);
                await _context.SaveChangesAsync();

                var isOldVideoUsed = await _context.MovieQualities.AnyAsync(q => q.VideoUrl == oldVideoUrl);
                if (!isOldVideoUsed && !string.IsNullOrEmpty(oldVideoUrl))
                {
                    await DeleteVideoFile(oldVideoUrl);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateMovieQualityAsync(EditMovieQualityViewModel model)
        {
            try
            {
                var quality = await _context.MovieQualities.FindAsync(model.Id);
                if (quality == null)
                    return false;

                // تحديث البيانات الأساسية
                quality.QualityType = model.QualityType;
                quality.Codec = model.Codec;
                quality.FileSize = (long)(model.FileSizeMB * 1024 * 1024);
                quality.IsAvailable = model.IsAvailable;

                // تحديث ملف الفيديو إذا تم رفع جديد
                if (model.VideoFile != null && model.VideoFile.Length > 0)
                {
                    var oldVideoUrl = quality.VideoUrl;

                    // رفع الملف الجديد
                    quality.VideoUrl = await VideoServices.UploadVideoAsync(model.VideoFile, _environment.WebRootPath);

                    var isOldVideoUsedByAnotherQuality = await _context.MovieQualities
                        .AnyAsync(q => q.Id != quality.Id && q.VideoUrl == oldVideoUrl);

                    if (!isOldVideoUsedByAnotherQuality && !string.IsNullOrEmpty(oldVideoUrl))
                    {
                        await DeleteVideoFile(oldVideoUrl);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                Console.WriteLine($"Error updating movie quality: {ex.Message}");
                return false;
            }
        }

        private async Task DeleteVideoFile(string videoUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(videoUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, videoUrl.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting video file: {ex.Message}");
                // لا نرمي استثناء هنا لأن حذف الملف ليس أساسياً
            }
        }

        public async Task<EditMovieQualityViewModel> GetQualityForEditAsync(int id)
        {
            var quality = await _context.MovieQualities
                .Include(q => q.Movie)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quality == null)
                return null;

            return new EditMovieQualityViewModel
            {
                Id = quality.Id,
                MovieId = quality.MovieId,
                MovieTitle = quality.Movie?.Title,
                QualityType = quality.QualityType,
                ExistingVideoUrl = quality.VideoUrl,
                Codec = quality.Codec,
                FileSizeMB = Math.Round(quality.FileSize / (1024.0 * 1024.0), 2),
                IsAvailable = quality.IsAvailable
            };
        }

        // ✅ Get Quality By Id
        public async Task<MovieQualityViewModel> GetQualityByIdAsync(int id)
        {
            var quality = await _context.MovieQualities
                .Include(q => q.Movie)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quality == null)
                return null;

            return new MovieQualityViewModel
            {
                Id = quality.Id,
                MovieId = quality.MovieId,
                Quality = quality.QualityType,
                VideoUrl = quality.VideoUrl,
                FileSize = FormatFileSize(quality.FileSize),
                ViewCount = quality.ViewCount,
                Codec = quality.Codec,
                CreatedAt = quality.CreatedAt,
                IsAvailable = quality.IsAvailable
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public async Task<List<QualityOptionViewModel>> GetAvailableQualitiesAsync(int movieId)
        {
            var qualities = await _context.MovieQualities
                .Where(q => q.MovieId == movieId && q.IsAvailable)
                .OrderBy(q => q.QualityType)
                .ToListAsync();

            return qualities.Select(q => new QualityOptionViewModel
            {
                QualityId = q.Id,
                QualityType = q.QualityType,
                DisplayName = GetQualityDisplayName(q.QualityType),
                VideoUrl = q.VideoUrl,
                FileSizeFormatted = FormatFileSize(q.FileSize),
                Codec = q.Codec,
                Bitrate = q.Bitrate,
                BadgeClass = GetQualityBadgeClass(q.QualityType),
                IsRecommended = q.QualityType == "720p" || q.QualityType == "1080p",
                ViewCount = q.ViewCount
            }).ToList();
        }

        public async Task<List<MovieQualityViewModel>> GetMovieQualitiesAsync(int movieId)
        {
            var qualities = await _context.MovieQualities
                .Where(q => q.MovieId == movieId)
                .OrderBy(q => q.QualityType)
                .ToListAsync();

            return qualities.Select(q => new MovieQualityViewModel
            {
                Id = q.Id,
                Quality = q.QualityType,
                VideoUrl = q.VideoUrl,
                FileSize = FormatFileSize(q.FileSize),
                ViewCount = q.ViewCount,
            }).ToList();
        }
        public async Task<MovieDetailsViewModel> GetMovieDetailsByIdAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Producer)
                .Include(m => m.Genre)
                .Include(m => m.ActorMovies).ThenInclude(am => am.Actor)
                .Include(m => m.Qualities)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return null;

            // ✅ استخدام النوع الصحيح
            var qualities = movie.Qualities.Select(q => new MovieQualityViewModel
            {
                Id = q.Id,
                Quality = q.QualityType,
                VideoUrl = q.VideoUrl,
                FileSize = FormatFileSize(q.FileSize),
                ViewCount = q.ViewCount
            }).ToList();

            return new MovieDetailsViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Duration = movie.Duration,
                Poster = movie.Poster,
                Rating = movie.Rating,
                AgeRating = movie.AgeRating,
                Country = movie.Country,
                Language = movie.Language,
                Year = movie.Year,
                Producer = movie.Producer != null ? new ProducerVM
                {
                    Name = movie.Producer.Name,
                    Image = movie.Producer.Image,
                    Bio = movie.Producer.Bio
                } : null,
                Genre = movie.Genre != null ? new MoviesByGenreViewModel
                {
                    GenreId = movie.Genre.Id,
                    GenreName = movie.Genre.Name
                } : null,

                // ✅ التصحيح هنا - استخدام MovieQualityViewModel
                Qualities = qualities,

                Actors = movie.ActorMovies.Select(am => new ActorVM
                {
                    Name = am.Actor.Name,
                    Bio = am.Actor.Bio,
                    Image = am.Actor.Image
                }).ToList()
            };
        }

        public async Task<string> GetRecommendedQualityAsync(int movieId)
        {
            var qualities = await _context.MovieQualities
                .Where(q => q.MovieId == movieId && q.IsAvailable)
                .Select(q => q.QualityType)
                .ToListAsync();

            // ✅ أولوية التوصيات الصحيحة: 1080p > 720p > 4K > 480p > 360p
            if (qualities.Contains("1080p")) return "1080p";
            if (qualities.Contains("720p"))  return "720p";
            if (qualities.Contains("4K"))    return "4K";
            if (qualities.Contains("480p"))  return "480p";
            if (qualities.Contains("360p"))  return "360p";

            return qualities.FirstOrDefault();
        }
        private string GetQualityDisplayName(string qualityType)
        {
            return qualityType switch
            {
                "360p" => "360p - جودة منخفضة",
                "480p" => "480p - جودة متوسطة",
                "720p" => "720p HD - جودة عالية",
                "1080p" => "1080p Full HD - جودة فائقة",
                "4K" => "4K Ultra HD - جودة استثنائية",
                _ => qualityType
            };
        }

        private string GetQualityBadgeClass(string qualityType)
        {
            return qualityType switch
            {
                "360p" => "badge-secondary",
                "480p" => "badge-info",
                "720p" => "badge-primary",
                "1080p" => "badge-success",
                "4K" => "badge-warning",
                _ => "badge-light"
            };
        }

        public async Task<MoviePlayerViewModel> GetMoviePlayerDataAsync(int movieId, string quality = null)
        {
            var movie = await _context.Movies
                .Include(m => m.Qualities)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null) return null;

            var availableQualities = await GetAvailableQualitiesAsync(movieId);
            if (!availableQualities.Any()) return null;

            var selectedQuality = availableQualities.FirstOrDefault(q => q.QualityType == quality)
                                  ?? availableQualities.FirstOrDefault(q => q.IsRecommended)
                                  ?? availableQualities.First();

            return new MoviePlayerViewModel
            {
                MovieId = movie.Id,
                Title = movie.Title,
                CurrentVideoUrl = selectedQuality.VideoUrl,
                CurrentQuality = selectedQuality.QualityType,
                AvailableQualities = availableQualities,
                PosterUrl = movie.Poster,
                Duration = ParseDurationToMinutes(movie.Duration)
            };
        }

        private static int ParseDurationToMinutes(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration)) return 0;
            if (int.TryParse(duration, out var mins)) return mins;
            var parts = duration.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int m))
            {
                return hours * 60 + m;
            }
            return 0;
        }
    }
}
