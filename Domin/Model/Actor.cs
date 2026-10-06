using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domin.Model
{
    /// <summary>
    /// Represents an actor entity
    /// </summary>
    public class Actor
    {
        /// <summary>Unique identifier for the actor</summary>
        public int Id { get; set; }

        /// <summary>Actor name - Required, 3 to 150 characters</summary>
        [Required(ErrorMessage = "Actor name is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Actor biography - Required, maximum 2000 characters</summary>
        [Required(ErrorMessage = "Biography is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Biography must be between 10 and 2000 characters")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>Actor profile image path</summary>
        public string? Image { get; set; }

        /// <summary>Collection of movies featuring this actor</summary>
        public List<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}
