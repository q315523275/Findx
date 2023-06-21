using Findx.Guids;

namespace Findx.Guid.NewId
{
    public class SequentialGuidGenerator: IGuidGenerator
    {
        public System.Guid Create()
        {
           return MassTransit.NewId.NextSequentialGuid();
        }
    }
}