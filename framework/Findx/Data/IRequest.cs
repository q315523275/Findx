namespace Findx.Data;

/// <summary>
///     请求接口标记
/// </summary>
public interface IRequest;

/// <summary>
///     请求接口标记
/// </summary>
public interface IRequest<TKey>: IRequest
{
    /// <summary>
    ///     编号
    /// </summary>
    public TKey Id { set; get; }
}