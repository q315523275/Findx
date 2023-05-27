using System.Threading.Tasks;
using Findx.RPC.Messages;

namespace Findx.RPC.Clients
{
    /// <summary>
    ///     客户端回调函数
    /// </summary>
    public interface IClientCallBack
    {
        int NewCallBackId();

        Task<Response> NewCallBackTask(int id, int timeout, string servantName, string funcName);

        void CallBack(Response msg);
    }
}