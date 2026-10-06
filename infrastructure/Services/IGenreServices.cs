using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace infrastructure.Services
{
    public  interface IGenreServices
    {
        Task<IEnumerable<Genre>> GetAllAsync();
        Task<Genre> CreateGenreAsync(CreateGenreVM vm);
        Task<bool> UpdateGenreAsync(int id, UpdateGenreVM vm);
        Task<bool> DeleteGenreAsync(int id);

        Task<Genre> GetByIdAsync(int id);
    }

    public class GenreService : IGenreServices 
    {

        private readonly DB _db;
        private readonly ILogger<GenreService> _logger;

        public GenreService(DB db, ILogger<GenreService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _db.Genres.AsNoTracking().ToListAsync();
        }

        public async Task<Genre> CreateGenreAsync(CreateGenreVM vm) 
        
        {
            try
            {
                var entity = new Genre
                {
                    Name = vm.Name,
                    NameAr = vm.NameAr,
                    Description = vm.Description,
                    Icon = vm.Icon
                };

                _db.Genres.Add(entity);
                await _db.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating genre: {Name}", vm.Name);
                return null;
            }
        }


        public async Task<bool> UpdateGenreAsync(int id ,UpdateGenreVM vm) 
        
        {
            try
            {
                var genre = await _db.Genres.FirstOrDefaultAsync(g => g.Id ==id);
                if (genre == null) return false;
                genre.Name = vm.Name;
                genre.NameAr = vm.NameAr;
                genre.Description = vm.Description;
                genre.Icon = vm.Icon;

                _db.Genres.Update(genre);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating genre ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteGenreAsync(int id) 
        {
            try
            {
                var genre = await _db.Genres.Include(g => g.Movies).FirstOrDefaultAsync(g => g.Id == id);
                if (genre == null) return false;

                if (genre.Movies != null && genre.Movies.Any())
                {
                    _logger.LogWarning("Cannot delete genre ID: {Id} because it is linked to {Count} movies", id, genre.Movies.Count);
                    return false;
                }

                _db.Genres.Remove(genre);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting genre ID: {Id}", id);
                return false;
            }
        }

        public async Task<Genre> GetByIdAsync(int id)
        {
            return await _db.Genres.FindAsync(id);
        }

    }
}
