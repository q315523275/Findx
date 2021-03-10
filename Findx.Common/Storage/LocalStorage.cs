using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace Findx.Storage
{
    public class LocalStorage : IStorage
    {
        private const string MediaRootFoler = "user-content";

        private readonly IHostEnvironment _hostEnvironment;

        public LocalStorage(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public string StorageName => "Local";

        public async Task DeleteMediaAsync(string fileName)
        {
            var filePath = Path.Combine(_hostEnvironment.ContentRootPath, MediaRootFoler, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }

        public string GetMediaUrl(string fileName)
        {
            return $"/{MediaRootFoler}/{fileName}";
        }

        public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
        {
            var filePath = Path.Combine(_hostEnvironment.ContentRootPath, MediaRootFoler, fileName);
            using (var output = new FileStream(filePath, FileMode.Create))
            {
                await mediaBinaryStream.CopyToAsync(output);
            }
        }
    }
}
