namespace Findx;

/// <summary>
/// 池
/// </summary>
public interface IPool<T>
{
    /// <summary>
    /// 获取
    /// </summary>
    /// <returns></returns>
    T Rent();

    /// <summary>
    /// 归还
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="clear">清除数据</param>
    void Return(T obj, bool clear = false);
}