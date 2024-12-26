namespace Findx.Data;

/// <summary>
///     数据部门机构接口
/// </summary>
public interface IDataDepartment<TOrg> where TOrg : struct
{
    /// <summary>
    ///     数据机构编号
    /// </summary>
    TOrg? OrgId { get; set; }
    
    /// <summary>
    ///     数据机构
    /// </summary>
    string OrgName { get; set; }
}