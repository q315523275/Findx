using System.Threading.Tasks;
namespace Findx.Messaging
{
    internal abstract class CommandHandlerWrapper
    {
        public abstract Task Handle(ICommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    }

    internal class CommandHandlerWrapperImpl<T> : CommandHandlerWrapper where T : ICommand
    {
        public override async Task Handle(ICommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var handlers = serviceProvider.GetServices<ICommandHandler<T>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync((T)command, cancellationToken);
            }
        }
    }
}
