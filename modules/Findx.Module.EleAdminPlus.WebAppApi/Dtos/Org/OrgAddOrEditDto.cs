using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Org;

/// <summary>
///     新增或编辑机构信息参数Dto
/// </summary>
public partial class OrgAddOrEditDto : IRequest<long>
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
    public long? OwnerId { get; set; }

    /// <summary>
    ///     负责人
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }
}