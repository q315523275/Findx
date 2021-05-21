using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.BackgroundTask
{
    public interface IBackgroundTask
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
