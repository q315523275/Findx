using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     租户 - 使用Findx平台的客户
/// </summary>
[Table(Name = "sys_tenants")]
[EntityExtension(DataSource = "system")]
public class SysTenantInfo : FullAuditedBase<long, long>, ISoftDeletable
{
    /// <summary>
    ///     租户id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     租户编码（唯一）
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     租户名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     行业类型
    /// </summary>
    public string Industry { get; set; }  // medical, education, retail, manufacturing等

    /// <summary>
    ///     联系人
    /// </summary>
    public string ContactPerson { get; set; }

    /// <summary>
    ///     联系电话
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    ///     联系邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    ///     公司地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    ///     管理员账号
    /// </summary>
    public string AdminUsername { get; set; }

    /// <summary>
    ///     最大用户数（0表示不限制）
    /// </summary>
    public int MaxUsers { get; set; }

    /// <summary>
    ///     存储空间配额（MB，0表示不限制）
    /// </summary>
    public long StorageQuota { get; set; }

    /// <summary>
    ///     已用存储空间（MB）
    /// </summary>
    public long UsedStorage { get; set; }

    /// <summary>
    ///     订阅开始时间
    /// </summary>
    public DateTime? SubscribeTime { get; set; }

    /// <summary>
    ///     到期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    ///     状态：0禁用 1正常 2过期 3欠费
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    ///     主题配置（JSON格式）
    /// </summary>
    public string ThemeConfig { get; set; }

    /// <summary>
    ///     Logo地址
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    ///     自定义域名
    /// </summary>
    public string CustomDomain { get; set; }

    /// <summary>
    ///     数据库连接字符串（如果使用独立数据库）
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    ///     数据隔离方式：0共享数据库 1独立Schema 2独立数据库
    /// </summary>
    public int IsolationType { get; set; } = 0;

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}