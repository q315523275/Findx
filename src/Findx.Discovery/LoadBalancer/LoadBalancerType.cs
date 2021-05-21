namespace Findx.Discovery.LoadBalancer
{
    public enum LoadBalancerType
    {
        Random,
        RoundRobin,
        LeastConnection,
        Hash,
        NoLoadBalancer
    }
}
