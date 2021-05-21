using System.Threading;
using System.Threading.Tasks;
namespace Findx.ExceptionHandling
{
    public class NullExceptionSubscriber : ExceptionSubscriber
    {
        public override Task HandleAsync(ExceptionNotificationContext context, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
