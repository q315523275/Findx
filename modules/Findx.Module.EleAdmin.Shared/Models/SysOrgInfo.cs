﻿using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Shared.Models;

/// <summary>
///     组织
/// </summary>
[Table(Name = "sys_org")]
[EntityExtension(DataSource = "system")]
public class SysOrgInfo : FullAuditedBase<Guid, Guid>, ISort, ISoftDeletable, ITenant, IResponse
{
    /// <summary>
    ///     机构id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public override Guid Id { get; set; }

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public Guid ParentId { get; set; } = Guid.Empty;

    /// <summary>
    ///     机构名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     机构全称
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    ///     机构代码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     机构类型
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     负责人id
    /// </summary>
    public Guid? LeaderId { get; set; } = Guid.Empty;

    /// <summary>
    ///     负责人昵称
    /// </summary>
    public string LeaderNickname { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

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

    /// <summary>
    ///     租户id
    /// </summary>
    public string TenantId { get; set; }
}