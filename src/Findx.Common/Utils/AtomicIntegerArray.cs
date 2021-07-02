namespace Findx.Utils
{
    /// <summary>
    /// 原子整型数据对象
    /// </summary>
    public class AtomicIntegerArray : AtomicReferenceArray<int>
    {
        public AtomicIntegerArray(int length)
            : base(length)
        {
        }
    }
}
