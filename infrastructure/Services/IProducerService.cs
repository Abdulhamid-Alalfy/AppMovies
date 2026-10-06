using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace infrastructure.Services
{
    public interface IProducerService
    {
        Task<IEnumerable<ProducerVM>> GetAllAsync();
        Task<ProducerVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(ProducerVM vm);
        Task<bool> UpdateAsync(EditProducerVM vm);
        Task<bool> DeleteAsync(int id);
    }

    public class ProducerService : IProducerService 
    {
        private readonly DB _context;
        private readonly ImageFileServices _imageFileServices;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(DB context, ImageFileServices imageFileService, ILogger<ProducerService> logger)
        {
            _context = context;
            _imageFileServices = imageFileService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProducerVM>> GetAllAsync()
        {
            return await _context.Producers.Select(e => new ProducerVM
            {
                Id=e.Id,
                Name=e.Name,
                Bio=e.Bio,
                Image=e.Image,

            }).ToListAsync();
        }

        public async Task<ProducerVM?> GetByIdAsync(int id)
        {
            var p= await _context.Producers.FindAsync(id);
            if (p == null) return null;
            return new ProducerVM
            {
                Id = p.Id,
                Name = p.Name,
                Bio = p.Bio,
                Image = p .Image,
            };
        }

        public async Task<bool> CreateAsync(ProducerVM vm)
        {
            try
            {
                if (vm == null) return false;

                string? imagePath = null;
                if (vm.ImagePath != null && vm.ImagePath.Length > 0)
                {
                    imagePath = await _imageFileServices.UploadImageAsync(vm.ImagePath);
                }

                var producer = new Producer
                {
                    Name = vm.Name,
                    Bio = vm.Bio,
                    Image = imagePath
                };

                await _context.Producers.AddAsync(producer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating producer: {Name}", vm?.Name);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(EditProducerVM vm)
        {
            try
            {
                var producer = await _context.Producers.FindAsync(vm.Id);
                if (producer == null) return false;

                if (vm.ImagePath != null && vm.ImagePath.Length > 0)
                {
                    if (!string.IsNullOrEmpty(producer.Image))
                        _imageFileServices.DeleteImage(producer.Image);

                    producer.Image = await _imageFileServices.UploadImageAsync(vm.ImagePath);
                }

                producer.Name = vm.Name;
                producer.Bio = vm.Bio;

                _context.Producers.Update(producer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating producer ID: {Id}", vm?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var producer = await _context.Producers.FindAsync(id);
                if (producer == null) return false;

                if (!string.IsNullOrEmpty(producer.Image))
                {
                    _imageFileServices.DeleteImage(producer.Image);
                }

                _context.Producers.Remove(producer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting producer ID: {Id}", id);
                return false;
            }
        }
    }
}
