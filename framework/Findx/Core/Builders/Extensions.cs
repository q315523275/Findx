using Findx.DependencyInjection;
using Findx.Jobs;

namespace Findx.Core.Builders;

/// <summary>
///     Findx构建扩展
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     添加核心模块
    /// </summary>
    internal static IFindxBuilder AddCoreModule(this IFindxBuilder builder)
    {
        builder.AddModule<FindxCoreModule>().AddModule<DependencyModule>().AddModule<JobModule>();

        return builder;
    }
}