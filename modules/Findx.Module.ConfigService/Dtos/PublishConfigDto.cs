using Findx.Data;

namespace Findx.Module.ConfigService.Dtos;

/// <summary>
/// 发布配置Dto
/// </summary>
public class PublishConfigDto: IRequest
{
    /// <summary>
    /// 记录编号
    /// </summary>
    public Guid? Id { get; set; }
    
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
    /// 是否Beta(灰度)
    /// </summary>
    public bool IsBeta { get; set; }
    
    /// <summary>
    /// 灰度限定Ip集合
    /// </summary>
    public string BetaIps { get; set; }
}