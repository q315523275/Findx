using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Component.DistributedConfigurationCenter.Models;

/// <summary>
///     应用信息表
/// </summary>
[Table("config_apps")]
[EntityExtension(DataSource = "config")]
public class AppInfo : FullAuditedBase<long, long>, ISoftDeletable, IResponse
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    [Column(name: "id")]
    public override long Id { get; set; }
    
    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     appid
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     密钥
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }
}