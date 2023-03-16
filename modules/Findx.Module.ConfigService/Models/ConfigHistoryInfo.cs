
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.ConfigService.Models;

/// <summary>
/// 配置中心
/// </summary>
[Table(Name = "config_info_history")]
[EntityExtension(DataSource = "config")]
public class ConfigHistoryInfo: FullAuditedBase<Guid, Guid>, ISoftDeletable, IResponse
{
    /// <summary>
    /// 主键id
    /// </summary>
    [Column(Name = "id", IsPrimary = true)]
    public override Guid Id { get; set; }
    
    /// <summary>
    /// 应用编号
    /// </summary>
    public string AppId { get; set; }
    
    /// <summary>
    /// 数据编号
    /// </summary>
    public string DataId { get; set; }

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
    /// 版本号
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Md5
    /// </summary>
    public string Md5 { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}