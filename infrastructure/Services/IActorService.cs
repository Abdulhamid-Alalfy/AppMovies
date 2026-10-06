using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace infrastructure.Services
{
    public interface IActorService
    {
        Task<IEnumerable<Actor>> GetAllAsync();
        Task<Actor?> GetByIdAsync(int id);
        Task<Actor?> CreateAsync(ActorVM actorVM);
        Task<bool> UpdateAsync(EditVM actorVM);
        Task<bool> DeleteAsync(int id);
    }

    public class ActorService : IActorService
    {
        private readonly DB _context;
        private readonly ImageFileServices _imageFileServices;
        private readonly ILogger<ActorService> _logger;

        public ActorService(DB context, ImageFileServices imageFileServices, ILogger<ActorService> logger)
        {
            _context = context;
            _imageFileServices = imageFileServices;
            _logger = logger;
        }

        public async Task<IEnumerable<Actor>> GetAllAsync()
        {
            return await _context.Actors.AsNoTracking().ToListAsync();
        }

        public async Task<Actor?> GetByIdAsync(int id)
        {
            return await _context.Actors.FindAsync(id);
        }

        public async Task<Actor?> CreateAsync(ActorVM actorVM)
        {
            try
            {
                if (actorVM == null) return null;

                string imagePath = null;
                if (actorVM.ImagePath != null)
                {
                    imagePath = await _imageFileServices.UploadImageAsync(actorVM.ImagePath);
                }

                var actor = new Actor
                {
                    Name = actorVM.Name,
                    Bio = actorVM.Bio,
                    Image = imagePath
                };

                await _context.Actors.AddAsync(actor);
                await _context.SaveChangesAsync();

                return actor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating actor: {Name}", actorVM?.Name);
                return null;
            }
        }

        public async Task<bool> UpdateAsync(EditVM actorVM)
        {
            try
            {
                var actor = await _context.Actors.FindAsync(actorVM.Id);
                if (actor == null) return false;

                if (actorVM.ImagePath != null && actorVM.ImagePath.Length > 0)
                {
                    if (!string.IsNullOrEmpty(actor.Image))
                    {
                        _imageFileServices.DeleteImage(actor.Image);
                    }

                    actor.Image = await _imageFileServices.UploadImageAsync(actorVM.ImagePath);
                }

                actor.Name = actorVM.Name;
                actor.Bio = actorVM.Bio;

                _context.Actors.Update(actor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating actor ID: {Id}", actorVM?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var actor = await _context.Actors.FindAsync(id);
                if (actor == null) return false;

                if (!string.IsNullOrEmpty(actor.Image))
                {
                    _imageFileServices.DeleteImage(actor.Image);
                }

                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting actor ID: {Id}", id);
                return false;
            }
        }
    }
}
