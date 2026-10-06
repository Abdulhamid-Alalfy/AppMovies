using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domin.Model
{
    /// <summary>
    /// Represents a movie genre/category
    /// </summary>
    public class Genre
    {
        /// <summary>Unique identifier for the genre</summary>
        public int Id { get; set; }

        /// <summary>Genre name in English - Required, 2 to 100 characters</summary>
        [Required(ErrorMessage = "Genre name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Genre name in Arabic - Required, 2 to 100 characters</summary>
        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Arabic name must be between 2 and 100 characters")]
        public string NameAr { get; set; } = string.Empty;

        /// <summary>Genre description - Optional, maximum 500 characters</summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>Genre icon/image path</summary>
        public string? Icon { get; set; }

        /// <summary>Collection of movies in this genre</summary>
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
