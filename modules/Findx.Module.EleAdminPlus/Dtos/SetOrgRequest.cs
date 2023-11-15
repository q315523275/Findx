using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     设置组织信息Dto模型
/// </summary>
public class SetOrgRequest : IRequest
{
    /// <summary>
    ///     机构id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public long ParentId { get; set; }

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
    public long? LeaderId { get; set; }

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
}