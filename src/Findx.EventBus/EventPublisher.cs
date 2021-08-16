using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly IEventStore _storage;


        public EventPublisher(IEventDispatcher dispatcher, IEventStore storage)
        {
            _dispatcher = dispatcher;
            _storage = storage;
            Transaction = new AsyncLocal<IEventTransaction>();
        }

        public AsyncLocal<IEventTransaction> Transaction { get; }

        public void Publish(IntegrationEvent integrationEvent)
        {
            Check.NotNull(integrationEvent, nameof(integrationEvent));

            if (Transaction.Value?.DbTransaction == null)
            {
                var eventMediumMessage = _storage.SavePublishedEvent(integrationEvent);
                _dispatcher.EnqueueToPublish(eventMediumMessage);
            }
            else
            {
                var transaction = (EventTransactionBase)Transaction.Value;

                var eventMediumMessage = _storage.SavePublishedEvent(integrationEvent, transaction.DbTransaction);

                transaction.AddToBuffer(eventMediumMessage);

                if (transaction.AutoCommit)
                {
                    transaction.Commit();
                }
            }
        }

        public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            Check.NotNull(integrationEvent, nameof(integrationEvent));

            if (Transaction.Value?.DbTransaction == null)
            {
                var eventMediumMessage = _storage.SavePublishedEvent(integrationEvent);
                await _dispatcher.EnqueueToPublishAsync(eventMediumMessage);
            }
            else
            {
                var transaction = (EventTransactionBase)Transaction.Value;

                var eventMediumMessage = _storage.SavePublishedEvent(integrationEvent, transaction.DbTransaction);

                transaction.AddToBuffer(eventMediumMessage);

                if (transaction.AutoCommit)
                {
                    transaction.Commit();
                }
            }
        }
    }
}
