using System.Threading.Tasks;
using Findx.RPC.Messages;

namespace Findx.RPC.Clients
{
    public interface IRpcClientFactory
    {
        Task<Response> SendAsync(Request req);
    }
}