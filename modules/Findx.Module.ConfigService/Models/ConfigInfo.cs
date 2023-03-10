
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.ConfigService.Models;

/// <summary>
/// 配置中心
/// </summary>
[Table(Name = "config_info")]
[EntityExtension(DataSource = "config")]
public class ConfigInfo: FullAuditedBase<Guid, Guid>, ISoftDeletable, IResponse
{
    /// <summary>
    /// 主键id
    /// </summary>
    [Column(Name = "id", IsPrimary = true)]
    public override Guid Id { get; set; }
    
    /// <summary>
    /// 数据编号
    /// </summary>
    public string DataId { get; set; }
    
    /// <summary>
    /// 组别编号
    /// </summary>
    public string GroupId { get; set; }
    
    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; }
    
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// 环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}