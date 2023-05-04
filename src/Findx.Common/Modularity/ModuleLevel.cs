namespace Findx.Modularity;

/// <summary>
///     模块等级
/// </summary>
public enum ModuleLevel
{
    /// <summary>
    ///     框架级别，表示涉及第三方组件的基础模块
    /// </summary>
    Framework = 10,

    /// <summary>
    ///     应用级别，表示涉及应用数据的基础模块
    /// </summary>
    Application = 20,

    /// <summary>
    ///     业务级别，表示涉及真实业务处理的模块
    /// </summary>
    Business = 30
}