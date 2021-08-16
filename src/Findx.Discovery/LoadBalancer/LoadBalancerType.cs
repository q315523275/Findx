namespace Findx.Discovery
{
    public enum LoadBalancerType
    {
        Random,
        RoundRobin,
        LeastConnection,
        // Hash,
        NoLoadBalancer
    }
}
