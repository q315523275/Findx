using Findx.RPC.Messages;
using System.Threading.Tasks;

namespace Findx.RPC.Clients
{
    public interface IRpcClientFactory
    {
        Task<Response> SendAsync(Request req);
    }
}
