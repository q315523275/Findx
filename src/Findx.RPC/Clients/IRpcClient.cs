using System.Net;
using System.Threading.Tasks;
using Findx.RPC.Messages;

namespace Findx.RPC.Clients
{
    public interface IRpcClient
    {
        Task SendAsync(EndPoint endPoint, Request request);
    }
}