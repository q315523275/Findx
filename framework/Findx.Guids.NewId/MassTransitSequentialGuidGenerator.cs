namespace Findx.Guids.NewId;

public class MassTransitSequentialGuidGenerator: IGuidGenerator
{
    public System.Guid Create()
    {
        return MassTransit.NewId.NextSequentialGuid();
    }
}