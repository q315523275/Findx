using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Shared.Models;

/// <summary>
///     登录日志
/// </summary>
[Table(Name = "sys_login_record")]
[EntityExtension(DataSource = "system")]
public class SysLoginRecordInfo : EntityBase<Guid>, ICreationAudited<Guid>, ITenant, IResponse
{
    /// <summary>
    ///     编号
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public override Guid Id { get; set; }

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
    ///     登录类型
    ///     <para>0登录成功, 1登录失败, 2退出登录, 3续签token</para>
    /// </summary>
    public int LoginType { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     租户编号
    /// </summary>
    public string TenantId { get; set; }
}