using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace infrastructure.Services
{
    public class MoviesService : IMoviesService
    {
        private readonly DB _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ImageFileServices _imageFileServices;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<MoviesService> _logger;

        public MoviesService(DB context, IWebHostEnvironment environment, ImageFileServices imageFileServices, INotificationService notificationService, IAuditLogService auditLogService, ILogger<MoviesService> logger)
        {
            _context = context;
            _environment = environment;
            _imageFileServices = imageFileServices;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

       
        public async Task<IEnumerable<MovieViewModel>> GetAllMoviesAsync()
        {
            var movies = await _context.Movies
                .Include(m => m.Producer)
                .Include(m => m.Genre)
                .Include(m => m.Qualities)
                .Include(m => m.ActorMovies)         
                    .ThenInclude(am => am.Actor)    
                .ToListAsync();

            return movies.Select(m => new MovieViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Poster = m.Poster,
                Duration = m.Duration,
                Rating = m.Rating,
                AgeRating = m.AgeRating,

                GenreName = m.Genre?.Name,
                ProducerId = m.ProducerId,
                ProducerName = m.Producer?.Name,       
                Year = m.Year,
                Actors = m.ActorMovies.Select(am => new ActorVM
                {

                    Name = am.Actor.Name,

                }).ToList(),
                AvailableQualities = m.Qualities.Select(q => q.QualityType).ToList()


            });
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

                Qualities = qualities,

                Actors = movie.ActorMovies.Select(am => new ActorVM
                {
                    Name = am.Actor.Name,
                    Bio = am.Actor.Bio,
                    Image = am.Actor.Image
                }).ToList()
            };
        }

        public async Task<bool> AddMovieAsync(CreateMovieViewModel model, string? userId = null)
        {
            try
            {
                string posterPath = null;
                string videoPath = null;

                if (model.PosterFile != null)
                {
                    posterPath = await _imageFileServices.UploadImageAsync(model.PosterFile);
                    Console.WriteLine($" Poster uploaded successfully: {posterPath}");
                }

                var movie = new Movie
                {
                    Title = model.Title,
                    Description = model.Description,
                    ProducerId = model.ProducerId,
                    Poster = posterPath,
                    Duration = ConvertMinutesToDurationFormat(model.Duration),
                    GenreId = model.GenreId,
                    Rating = model.Rating,
                    AgeRating = model.AgeRating,
                    Country = model.Country,
                    Language = model.Language,
                    Year = model.Year!.Value
                };

                if (model.SelectedActorIds?.Any() == true)
                {
                    movie.ActorMovies = model.SelectedActorIds
                        .Select(aid => new ActorMovie { ActorId = aid })
                        .ToList();
                }

      
                await _context.Movies.AddAsync(movie);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($" SaveChanges result = {result}");

    
                if (result == 0)
                {
                    Console.WriteLine("⚠️ لم يتم حفظ أي تغييرات في قاعدة البيانات!");
                    return false;
                }

                try
                {
                    await _auditLogService.LogAsync(userId ?? "system", "Create", "Movie", movie.Id, $"Created movie: {movie.Title}");
                }
                catch { }

                try
                {
                    await _notificationService.CreateNotificationForAllUsersAsync(
                        title: " فيلم جديد",
                        message: $"تم إضافة فيلم جديد: {movie.Title}",
                        link: $"/Movie/Details/{movie.Id}",
                        type: NotificationType.NewMovie
                    );
                }
                catch (Exception exNotif)
                {
                    Console.WriteLine($"⚠️ فشل إرسال الإشعار: {exNotif.Message}");
                }

                Console.WriteLine(" Movie added successfully!");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding movie: {Message}", ex.Message);
                return false;
            }
        }

       
        public async Task<UpdateMovieModelView> GetMovieForEditAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return null;

            return new UpdateMovieModelView
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ProducerId = movie.ProducerId,
                Duration = ConvertDurationToMinutes(movie.Duration),
                Poster = movie.Poster,
                GenreId = movie.GenreId,
                Rating = movie.Rating,
                AgeRating = movie.AgeRating,
                Country = movie.Country,
                Language = movie.Language,
                Year = movie.Year,
                SelectedActorIds = movie.ActorMovies.Select(am => am.ActorId).ToList()
            };
        }


        public async Task<bool> UpdateMovieAsync(UpdateMovieModelView model, string? userId = null)
        {
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.ActorMovies)
                    .FirstOrDefaultAsync(m => m.Id == model.Id);

                if (movie == null)
                    return false;

  
                movie.Title = model.Title;
                movie.Description = model.Description;
                movie.ProducerId = model.ProducerId;
                movie.Duration = ConvertMinutesToDurationFormat(model.Duration);
                movie.GenreId = model.GenreId;
                movie.Rating = model.Rating;
                movie.AgeRating = model.AgeRating;
                movie.Country = model.Country;
                movie.Language = model.Language;
                movie.Year = model.Year!.Value;

                if (model.PosterFile != null)
                {
                    movie.Poster = await _imageFileServices.UploadImageAsync(model.PosterFile);
                }

   
                if (model.VideoFile != null)
                {
                    movie.VideoPath = await VideoServices.UploadVideoAsync(model.VideoFile, _environment.WebRootPath);
                }


                _context.ActorMovies.RemoveRange(movie.ActorMovies);

                if (model.SelectedActorIds?.Any() == true)
                {
                    movie.ActorMovies = model.SelectedActorIds
                        .Select(aid => new ActorMovie { ActorId = aid, MovieId = movie.Id })
                        .ToList();
                }

                await _context.SaveChangesAsync();
                try
                {
                    await _auditLogService.LogAsync(userId ?? "system", "Update", "Movie", movie.Id, $"Updated movie: {movie.Title}");
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating movie ID: {MovieId}", model.Id);
                return false;
            }
        }


        public async Task<bool> DeleteMovieAsync(int id, string? userId = null)
        {
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.ActorMovies)
                    .Include(m => m.Qualities)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movie == null) return false;

                _context.ActorMovies.RemoveRange(movie.ActorMovies);
                _context.RemoveRange(movie.Qualities);
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                try
                {
                    await _auditLogService.LogAsync(userId ?? "system", "Delete", "Movie", movie.Id, $"Deleted movie: {movie.Title}");
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting movie ID: {MovieId}", id);
                return false;
            }
        }


        public async Task<List<ProducerListVM>> GetAllProducersAsync()
        {
            return await _context.Producers
                .Select(p => new ProducerListVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    Bio = p.Bio,
                }).ToListAsync();
        }


        public async Task<List<ActorListVm>> GetAllActorsAsync()
        {
            return await _context.Actors
                .Select(a => new ActorListVm
                {
                    Id= a.Id,
                    Name = a.Name,
                    Bio = a.Bio,
                    Image = a.Image,
                }).ToListAsync();
        }

      

        public async Task<IEnumerable<MovieViewModel>> SearchMoviesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                searchTerm = "";

            var movies = await _context.Movies
                .Include(m => m.Producer)
                .Include(m => m.Genre)
                .Where(m => m.Title.Contains(searchTerm) ||
                            m.Description.Contains(searchTerm) ||
                            m.Producer.Name.Contains(searchTerm) ||
                            m.Genre.Name.Contains(searchTerm))
                .ToListAsync();

            return movies.Select(m => new MovieViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Poster = m.Poster,
                Duration = m.Duration,
                Rating = m.Rating,
                AgeRating = m.AgeRating,
                GenreName = m.Genre?.Name,
                ProducerId = m.ProducerId,
                Year = m.Year
            });
        }

        public async Task<List<GenreListViewModel>> GetAllGenresAsync()
        {
            return await _context.Genres
                .Select(g => new GenreListViewModel
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToListAsync();
        }
        public async Task<MoviesByGenreViewModel> GetMoviesByGenreWithPaginationAsync(int genreId, int page = 1, int pageSize = 12)
        {

            var genre = await _context.Genres.FindAsync(genreId);

            if (genre == null)
                return null;


            var totalMovies = await _context.Movies
                .Where(m => m.GenreId == genreId)
                .CountAsync();

            var movies = await _context.Movies
                .Where(m => m.GenreId == genreId)
                .OrderByDescending(m => m.Year)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.Genre)
                .Include(m => m.Producer)
                .ToListAsync();

            var viewModel = new MoviesByGenreViewModel
            {
                GenreId = genre.Id,
                GenreName = genre.Name,
                GenreNameAr = genre.NameAr ?? genre.Name,
                GenreDescription = genre.Description ?? "",
                Movies = movies.Select(m => new MovieCardViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.Poster,
                    ReleaseDate = new DateTime(m.Year, 1, 1), // استخدام Year
                    Rating = double.TryParse(m.Rating, out double rate) ? rate : 0,
                    Duration = int.TryParse(ConvertDurationToMinutes(m.Duration), out int dur) ? dur : 0,
                    GenreName = m.Genre?.Name
                }).ToList(),
                TotalMovies = totalMovies,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize)
            };

            return viewModel;
        }

  
        public async Task<List<GenreListViewModel>> GetAllGenresWithCountAsync()
        {
            return await _context.Genres
                .Select(g => new GenreListViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    NameAr = g.NameAr ?? g.Name,
                    Icon = g.Icon ?? "fa-film",
                    MovieCount = g.Movies.Count()
                })
                .ToListAsync();
        }

        private string ConvertMinutesToDurationFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            if (input.Contains(":"))
                return input;

            if (int.TryParse(input, out int totalMinutes))
            {
                var hours = totalMinutes / 60;
                var minutes = totalMinutes % 60;
                return $"{hours:D2}:{minutes:D2}:00";
            }

            return input;
        }
        private string ConvertDurationToMinutes(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration) || !duration.Contains(":"))
                return duration;

            var parts = duration.Split(':');
            if (parts.Length == 3 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int mins))
            {
                return (hours * 60 + mins).ToString();
            }

            return duration;
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

        public async Task<IEnumerable<MovieViewModel>> GetLatestMoviesAsync(int count) 
        {
            var new_movie = await _context.Movies
                .OrderByDescending(m => m.Id)
                .Take(count)
                .Select(m => new MovieViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Poster = m.Poster,
                    Duration = m.Duration,
                    Rating = m.Rating,
                    AgeRating = m.AgeRating,
                    Year = m.Year,
                }).ToListAsync();

            return new_movie;
        }
    }
}
