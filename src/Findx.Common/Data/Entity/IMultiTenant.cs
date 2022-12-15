namespace Findx.Data
{
    /// <summary>
    /// 多租户
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        Guid? TenantId { get; set; }
    }

    /// <summary>
    /// 多租户
    /// </summary>
    public interface ITenant<TKey> where TKey : struct
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        TKey? TenantId { get; set; }
    }
}
