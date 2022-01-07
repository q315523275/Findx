namespace Findx.Data
{
    /// <summary>
    /// 租户
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ITenant<TKey> where TKey : struct
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        TKey? TenantId { get; set; }
    }
}
