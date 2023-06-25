namespace Findx.Guids.NewId
{
    public class SequentialGuidGenerator: IGuidGenerator
    {
        public System.Guid Create()
        {
           return MassTransit.NewId.NextSequentialGuid();
        }
    }
}