namespace Findx.Common;

/// <summary>
///     异步标识接口
/// </summary>
public interface IAsync
{
}

/// <summary>
///     异步标识属性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AsyncAttribute : Attribute
{
}