namespace Findx.Discovery.LoadBalancer
{
    public class Lease
    {
        public Lease(IServiceInstance serviceInstance, int connections)
        {
            ServiceInstance = serviceInstance;
            Connections = connections;
        }
        public IServiceInstance ServiceInstance { get; private set; }
        public int Connections { get; private set; }
    }
}
