using System.IO;
using System.Threading.Tasks;

namespace Findx.Storage
{
    public interface IStorage
    {
        string StorageName { get; }

        string GetMediaUrl(string fileName);

        Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);

        Task DeleteMediaAsync(string fileName);
    }
}
