using Findx.EventBus.Events;
using System;
using System.Threading.Tasks;

namespace Findx.EventBus.Abstractions
{
    public interface IEventStore
    {
        Task RetrieveEventLogsPendingToPublishAsync();
        Task SaveEventAsync(IntegrationEvent @event, object transaction = null);
        Task MarkEventAsPublishedAsync(Guid eventId);
        Task MarkEventAsInProgressAsync(Guid eventId);
        Task MarkEventAsFailedAsync(Guid eventId);
    }
}