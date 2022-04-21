namespace Findx.Storage
{
    public interface IStorageProvider
    {
        IFileStorage Get(string storageName = null);
    }
}
