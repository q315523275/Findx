namespace Findx.NewId;

/// <summary>
///     Guid主键生成器
/// </summary>
public class GuidKeyGenerator : IKeyGenerator<Guid>
{
    private readonly IGuidGenerator _generator;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="generator"></param>
    public GuidKeyGenerator(IGuidGenerator generator)
    {
        _generator = generator;
    }

    /// <summary>
    ///     获取一个<see cref="Guid" />类型的主键数据
    /// </summary>
    /// <returns></returns>
    public Guid Create()
    {
        return _generator.Create();
    }
}