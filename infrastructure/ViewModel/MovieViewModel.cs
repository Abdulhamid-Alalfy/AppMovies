using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domin.Model;
using Microsoft.AspNetCore.Http;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace infrastructure.ViewModel
{
    // 🧩 كلاس أساسي يحتوي على الخصائص المشتركة
    public abstract class BaseMovieViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public int ProducerId { get; set; }

        [Required]
        public int GenreId { get; set; }

        [Required]
        public string Duration { get; set; }

        public string Rating { get; set; }

        public string AgeRating { get; set; }

        [Required]
        public string Country { get; set; }

        public string Language { get; set; } = "en";

        [Required(ErrorMessage = "سنة الإصدار مطلوبة")]
        [Range(1900, 2099, ErrorMessage = "سنة الإصدار يجب أن تكون بين 1900 و 2099")]
        public int? Year { get; set; }

        public List<int> SelectedActorIds { get; set; } = new List<int>();

        // Properties for Dropdowns
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ProducersList { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ActorsList { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GenresList { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }

    // 🎬 MovieViewModel - للعرض العام
    public class MovieViewModel : BaseMovieViewModel
    {
        public string ProducerName { get; set; }
        public string Poster { get; set; }
        public string VideoPath { get; set; }
        public string GenreName { get; set; }
        public List<string> AvailableQualities { get; set; }
        public QualityOptionViewModel DefaultQuality { get; set; }
        public List<ActorVM> Actors { get; set; } = new List<ActorVM>();
    }

    // 🎥 CreateMovieViewModel - لإنشاء فيلم جديد
    public class CreateMovieViewModel : BaseMovieViewModel
    {
        public string? Poster { get; set; }
        public IFormFile PosterFile { get; set; }
        
    }

    // 🧰 UpdateMovieModelView - لتعديل فيلم موجود
    public class UpdateMovieModelView : BaseMovieViewModel
    {
        public string? Poster { get; set; }
        public IFormFile? PosterFile { get; set; }
        public IFormFile? VideoFile { get; set; }
        public List<ActorVM> Actors { get; set; } = new List<ActorVM>();
    }

    // 📖 MovieDetailsViewModel - تفاصيل الفيلم
    public class MovieDetailsViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Poster { get; set; }

        public string VideoPath { get; set; }

        public int Year { get; set; } = DateTime.Now.Year;

        public string Duration { get; set; }

        public string Country { get; set; }

        public string Language { get; set; } = "en";

        public string Rating { get; set; }

        public string AgeRating { get; set; }

        public ProducerVM Producer { get; set; }
        public MoviesByGenreViewModel Genre { get; set; }

        public List<MovieQualityViewModel> Qualities { get; set; }
        public List<ActorVM> Actors { get; set; } = new();
    }

    public class MoviesByGenreViewModel
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public string GenreNameAr { get; set; }
        public string GenreDescription { get; set; }
        public List<MovieCardViewModel> Movies { get; set; }
        public int TotalMovies { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // ViewModels/MovieCardViewModel.cs
    public class MovieCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PosterUrl { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double Rating { get; set; }
        public int Duration { get; set; }
        public string GenreName { get; set; }
    }

    // ViewModels/GenreListViewModel.cs
    public class GenreListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Icon { get; set; }
        public int MovieCount { get; set; }
    }

    public class CreateGenreVM 
    {

        public string Name { get; set; }
        public string NameAr { get; set; } // الاسم بالعربي
        public string Description { get; set; }
        public string Icon { get; set; } // أيقونة النوع

    }

    public class UpdateGenreVM : CreateGenreVM { public int Id { get; set; } }

    public class RequiredForNewAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var movie = validationContext.ObjectInstance as BaseMovieViewModel;
            if (movie != null && movie.Id == 0 && value == null)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
