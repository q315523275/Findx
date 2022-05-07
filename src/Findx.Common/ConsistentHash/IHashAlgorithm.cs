namespace Findx.ConsistentHash
{
    public interface IHashAlgorithm
    {
        int Hash(string item);
    }
}

