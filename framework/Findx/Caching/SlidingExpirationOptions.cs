using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Caching;

/// <summary>
///     缓存滑动过期配置
/// </summary>
public class SlidingExpirationOptions: ValidatableObject
{
    /// <summary>
    ///     滑动时间
    /// </summary>
    [Required]
    public TimeSpan SlidingExpiration { set; get; }
}