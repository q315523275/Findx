using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Storage
{
    public interface IStorage
    {
        string StorageName { get; }

        string GetMediaUrl(string fileName);

        Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null, CancellationToken token = default);

        Task DeleteMediaAsync(string fileName, CancellationToken token = default);
    }
}
