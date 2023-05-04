//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Findx.EventBus;
//using Findx.WebHost.EventBus;
//using Microsoft.Extensions.Hosting;

//namespace Findx.WebHost.RabbitMQ
//{
//    public class EventBusWorker : BackgroundService
//    {
//        private readonly IEventSubscriber _eventBus;

//        public EventBusWorker(IEventSubscriber eventBus)
//        {
//            _eventBus = eventBus;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            await Task.Yield();

//            _eventBus.Subscribe<FindxTestEvent, FindxTestEventHander>();

//            _eventBus.Subscribe<FindxTestEvent, FindxTestEventHanderTwo>();

//            _eventBus.SubscribeDynamic<FindxTestDynamicEventHandler>("Findx.WebHost.EventBus.FindxTestEvent");

//            _eventBus.StartConsuming();
//        }
//    }

//}

