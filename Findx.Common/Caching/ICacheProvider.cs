namespace Findx.Caching
{
    public interface ICacheProvider
    {
        ICache Get(CacheType name = CacheType.Memory);
    }
}
