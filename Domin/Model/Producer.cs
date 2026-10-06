using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domin.Model
{
    /// <summary>
    /// Represents a movie producer entity
    /// </summary>
    public class Producer
    {
        /// <summary>Unique identifier for the producer</summary>
        public int Id { get; set; }

        /// <summary>Producer profile image path</summary>
        public string? Image { get; set; }

        /// <summary>Producer name - Required, 3 to 150 characters</summary>
        [Required(ErrorMessage = "Producer name is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Producer biography - Required, maximum 2000 characters</summary>
        [Required(ErrorMessage = "Biography is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Biography must be between 10 and 2000 characters")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>Collection of movies produced by this producer</summary>
        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}
