using System;
namespace Findx.Guids
{
    /// <summary>
    /// 原始Guid生成
    /// </summary>
    public class SimpleGuidGenerator : IGuidGenerator
    {
        public static SimpleGuidGenerator Instance { get; } = new();

        public virtual Guid Create()
        {
            return Guid.NewGuid();
        }

    }
}

