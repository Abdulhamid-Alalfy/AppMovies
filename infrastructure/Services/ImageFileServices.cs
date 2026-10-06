using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace infrastructure.Services
{
    public class ImageFileServices
    {
        private  IWebHostEnvironment _env;
        public ImageFileServices(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> UploadImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null)
                return null;

            string uploadsFolder = Path.Combine(_env.WebRootPath, "Images");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // استخدام النسخة async
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/Images/" + uniqueFileName;
        }



        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;       
            var relativePath = imagePath.TrimStart('/');
         
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

    }
}
