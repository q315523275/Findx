using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.OperationRecord;

/// <summary>
///     操作记录Dto
/// </summary>
public class OperationRecordDto: IResponse
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     账号
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     用户名
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     操作系统
    /// </summary>
    public string Os { get; set; }

    /// <summary>
    ///     设备名
    /// </summary>
    public string Device { get; set; }

    /// <summary>
    ///     浏览器类型
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    ///     ip地址
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     状态, 0成功, 1异常
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    ///     模块
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    ///     操作功能
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     请求地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     请求方式
    /// </summary>
    public string RequestMethod { get; set; }

    /// <summary>
    ///     请求方法
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    ///     异常信息
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    ///     消耗时间, 单位毫秒
    /// </summary>
    public int SpendTime { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}