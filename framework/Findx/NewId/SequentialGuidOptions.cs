using Findx.Utilities;

namespace Findx.NewId;

/// <summary>
///     有序Guid生成配置
/// </summary>
public class SequentialGuidOptions : IOptions<SequentialGuidOptions>
{
    /// <summary>
    /// </summary>
    public SequentialGuidType? SequentialGuidType { set; get; }

    /// <summary>
    ///     this
    /// </summary>
    public SequentialGuidOptions Value => this;
}