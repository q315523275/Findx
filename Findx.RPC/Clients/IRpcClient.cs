using Findx.RPC.Messages;
using System.Net;
using System.Threading.Tasks;

namespace Findx.RPC.Clients
{
    public interface IRpcClient
    {
        Task SendAsync(EndPoint endPoint, Request request);
    }
}
