namespace Findx.Messaging
{
    public interface IApplicationEvent { }
    public interface IApplicationEvent<out TResponse> : IApplicationEvent { }
}
