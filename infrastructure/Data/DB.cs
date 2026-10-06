using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Data
{
    public class DB : IdentityDbContext<AppUser>
    {
        public DB(DbContextOptions<DB> options) : base(options)
        {
        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<ActorMovie> ActorMovies { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<MovieQuality> MovieQualities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WatchHistory> WatchHistory { get; set; }
        public DbSet<UserNotificationRead> UserNotificationReads { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-Many relation between Actor and Movie
            modelBuilder.Entity<ActorMovie>()
                .HasKey(am => new { am.ActorId, am.MovieId });

            modelBuilder.Entity<ActorMovie>()
                .HasOne(am => am.Actor)
                .WithMany(a => a.ActorMovies)
                .HasForeignKey(am => am.ActorId);

            modelBuilder.Entity<ActorMovie>()
                .HasOne(am => am.Movie)
                .WithMany(m => m.ActorMovies)
                .HasForeignKey(am => am.MovieId);

            // Movie to Genre relationship
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasOne(m => m.Genre)
                      .WithMany(g => g.Movies)
                      .HasForeignKey(m => m.GenreId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Producer)
                      .WithMany(p => p.Movies)
                      .HasForeignKey(m => m.ProducerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // UserFavorite relations
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(uf => new { uf.UserId, uf.MovieId })
                .IsUnique();

            modelBuilder.Entity<UserFavorite>()
                .HasOne(f => f.Movies)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId);

            // WatchHistory configuration
            modelBuilder.Entity<WatchHistory>()
                .HasIndex(wh => new { wh.UserId, wh.MovieId })
                .IsUnique();

            modelBuilder.Entity<WatchHistory>()
                .HasOne(wh => wh.Movie)
                .WithMany()
                .HasForeignKey(wh => wh.MovieId);

            modelBuilder.Entity<WatchHistory>()
                .HasOne(wh => wh.User)
                .WithMany(u => u.WatchHistories)
                .HasForeignKey(wh => wh.UserId);

            // UserNotificationRead configuration
            modelBuilder.Entity<UserNotificationRead>()
                .HasIndex(unr => new { unr.UserId, unr.NotificationId })
                .IsUnique();
        }
    }
}
