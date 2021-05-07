namespace Findx.Messaging
{
    public interface IMessageRequest { }
    public interface IMessageRequest<out TResponse> : IMessageRequest { }
}
