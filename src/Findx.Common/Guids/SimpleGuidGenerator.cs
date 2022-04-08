using System;
namespace Findx.Guids
{
    public class SimpleGuidGenerator : IGuidGenerator
    {
        public static SimpleGuidGenerator Instance { get; } = new SimpleGuidGenerator();

        public virtual Guid Create()
        {
            return Guid.NewGuid();
        }

    }
}

