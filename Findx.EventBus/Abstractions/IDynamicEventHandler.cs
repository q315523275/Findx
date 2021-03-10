using System.Threading.Tasks;

namespace Findx.EventBus.Abstractions
{
    public interface IDynamicEventHandler
    {
        Task HandleAsync(string eventData);
    }
}
