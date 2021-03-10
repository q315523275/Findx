using System.Threading.Tasks;
namespace Findx.ExceptionHandling
{
    public class NullExceptionSubscriber : ExceptionSubscriber
    {
        public override Task HandleAsync(ExceptionNotificationContext context)
        {
            return Task.CompletedTask;
        }
    }
}
