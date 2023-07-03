namespace Findx.Discovery.LoadBalancer
{
    /// <summary>
    /// 服务负载计算-连接少优先
    /// </summary>
    public class Lease
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="connections"></param>
        public Lease(IServiceInstance serviceInstance, int connections)
        {
            ServiceInstance = serviceInstance;
            Connections = connections;
        }

        /// <summary>
        /// 服务实例
        /// </summary>
        public IServiceInstance ServiceInstance { get; private set; }
        
        /// <summary>
        /// 连接数
        /// </summary>
        public int Connections { get; private set; }
    }
}