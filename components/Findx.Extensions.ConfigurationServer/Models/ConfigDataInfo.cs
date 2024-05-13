using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Models;

/// <summary>
///     配置信息表
/// </summary>
[Table("config_info")]
[EntityExtension(DataSource = "config")]
public class ConfigDataInfo : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    [Column(name: "id")]
    public override long Id { get; set; }
    
    /// <summary>
    ///     应用编号
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     数据编号
    /// </summary>
    public string DataId { get; set; }

    /// <summary>
    ///     数据类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    ///     内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    ///     版本号
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    ///     Md5
    /// </summary>
    public string Md5 { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string Comments { get; set; }
}