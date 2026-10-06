using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domin.Enum;

namespace Domin.Model
{
    /// <summary>
    /// Represents a movie entity with all its metadata
    /// </summary>
    public class Movie
    {
        /// <summary>Unique identifier for the movie</summary>
        public int Id { get; set; }

        /// <summary>Movie title - Required, maximum 200 characters</summary>
        [Required(ErrorMessage = "Movie title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Movie description - Optional, maximum 2000 characters</summary>
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Producer ID - Required</summary>
        [Required(ErrorMessage = "Producer is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid producer")]
        public int ProducerId { get; set; }

        /// <summary>Navigation property to Producer</summary>
        public Producer? Producer { get; set; }

        /// <summary>Movie poster image path</summary>
        public string? Poster { get; set; }

        /// <summary>Movie video file path</summary>
        public string? VideoPath { get; set; }

        /// <summary>Release year - Must be between 1900 and current year</summary>
        [Range(1900, 2099, ErrorMessage = "Year must be between 1900 and 2099")]
        public int Year { get; set; } = DateTime.Now.Year;

        /// <summary>Movie duration in format HH:MM:SS - Required</summary>
        [Required(ErrorMessage = "Duration is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Duration must be in HH:MM:SS format")]
        public string Duration { get; set; } = string.Empty;

        /// <summary>Movie production country - Required</summary>
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;

        /// <summary>Movie language - Optional, default English</summary>
        [StringLength(50, ErrorMessage = "Language cannot exceed 50 characters")]
        public string Language { get; set; } = "en";

        /// <summary>User favorites collection</summary>
        public virtual List<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();

        /// <summary>Movie rating (e.g., 7.5/10) - Optional</summary>
        [RegularExpression(@"^[0-9]+(\.[0-9]{1,2})?$", ErrorMessage = "Rating must be a valid decimal number")]
        public string? Rating { get; set; }

        /// <summary>Age rating (e.g., PG-13, R, etc.) - Optional</summary>
        [StringLength(20, ErrorMessage = "Age rating cannot exceed 20 characters")]
        public string? AgeRating { get; set; }

        /// <summary>Genre ID - Required</summary>
        [Required(ErrorMessage = "Genre is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid genre")]
        public int GenreId { get; set; }

        /// <summary>Navigation property to Genre</summary>
        public Genre? Genre { get; set; }

        /// <summary>Collection of movie qualities (resolutions)</summary>
        public ICollection<MovieQuality> Qualities { get; set; } = new List<MovieQuality>();

        /// <summary>Collection of actors in this movie</summary>
        public List<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}