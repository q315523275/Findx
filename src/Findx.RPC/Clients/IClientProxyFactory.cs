using System;

namespace Findx.RPC.Clients
{
    public interface IClientProxyFactory
    {
        object Create(Type type);
    }
}