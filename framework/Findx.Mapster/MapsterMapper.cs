using Findx.Mapping;
using Mapster;

namespace Findx.Mapster;

public class MapsterMapper : IObjectMapper
{
    public TTarget MapTo<TTarget>(object source)
    {
        return source.Adapt<TTarget>();
    }

    public TTarget MapTo<TSource, TTarget>(TSource source, TTarget target)
    {
        source.Adapt(target);
        return target;
    }
}