namespace Findx.Data
{
    /// <summary>
    /// 多租户
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IMultiTenant<TKey> where TKey : struct
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        TKey? TenantId { get; set; }
    }
}
