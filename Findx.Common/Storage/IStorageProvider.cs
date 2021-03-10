namespace Findx.Storage
{
    public interface IStorageProvider
    {
        IStorage Get(string storageName = "Local");
    }
}
