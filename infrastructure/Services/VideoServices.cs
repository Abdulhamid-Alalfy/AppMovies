using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace infrastructure.Services
{
    public class VideoServices
    {
        public static async Task<string> UploadVideoAsync(IFormFile videoFile, string webRootPath)
        {
            if (videoFile == null || videoFile.Length == 0) return null;

            string uploadsFolder = Path.Combine(webRootPath, "videos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileExtension = Path.GetExtension(videoFile.FileName).ToLower();
            if (!IsValidVideoExtension(fileExtension))
                throw new InvalidOperationException("Invalid video file format.");

            string uniqueFileName;
            string filePath;
            do
            {
                uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);
            } while (File.Exists(filePath));

            // استخدام FileStream بوضع async و buffer مناسب
            const int bufferSize = 81920; // 80 KB
            using (var fileStream = new FileStream(
                       filePath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: bufferSize,
                       useAsync: true)) // مهم: useAsync = true
            {
                await videoFile.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return "/videos/" + uniqueFileName;
        }

        private static bool IsValidVideoExtension(string extension)
        {
            string[] validExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi" };
            return Array.Exists(validExtensions, e => e == extension);
        }
    }
}
