namespace Findx.Caching
{
    public interface ICacheProvider
    {
        ICache Get(string name = CacheType.DefaultMemory);
    }
}
