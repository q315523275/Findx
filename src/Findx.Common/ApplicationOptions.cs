namespace Findx;

/// <summary>
/// 应用配置
/// </summary>
public class ApplicationOptions: IOptions<ApplicationOptions>
{
    /// <summary>
    /// value
    /// </summary>
    public ApplicationOptions Value => this;

    /// <summary>
    /// 应用id
    /// </summary>
    public string Id { set; get; }
    
    /// <summary>
    /// 应用名称
    /// </summary>
    public string Name { set; get; }
    
    /// <summary>
    /// 普通端口
    /// </summary>
    public int Port { set; get; }
    
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { set; get; }
    
    /// <summary>
    /// 绑定网址
    /// </summary>
    public string Uris { set; get; }
    
    /// <summary>
    /// 服务ip
    /// </summary>
    public string InstanceIp { set; get; }
    
    /// <summary>
    /// 内网ip
    /// </summary>
    public string InternalIp { set; get; }
    
    /// <summary>
    /// 是否验证端口
    /// </summary>
    public bool AvailablePort { set; get; }
}