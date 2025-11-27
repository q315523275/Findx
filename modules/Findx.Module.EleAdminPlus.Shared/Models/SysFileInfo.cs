using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     文件信息表
/// </summary>
[Table(Name = "sys_file_record")]
[EntityExtension(DataSource = "system")]
public partial class SysFileInfo: FullAuditedBase<long, long>, IDataDepartment<long>, ITenant, ISoftDeletable
{
    /// <summary>
    ///     文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件路径
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     文件大小(kb)
    /// </summary>
    public long Length { get; set; }
    
    /// <summary>
    ///     文件网址
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    ///     缩略图网址
    /// </summary>
    public string Thumbnail { get; set; }
    
    /// <summary>
    ///     下载地址
    /// </summary>
    public string DownloadUrl { get; set; }
    
    /// <summary>
    ///     内容类型
    /// </summary>
    public string ContentType { get; set; }
    
    /// <summary>
    ///     扩展名称
    /// </summary>
    public string Extension { get; set; }
    
    /// <summary>
    ///     文件类型
    /// </summary>
    public int FileType { get; set; }
    
    /// <summary>
    ///     文件类型Id
    /// </summary>
    public long? FileTypeId { get; set; }
    
    /// <summary>
    ///     文件类型Id名称
    /// </summary>
    public string FileTypeName { get; set; }
    
    /// <summary>
    ///     机构Id
    /// </summary>
    public long? OrgId { get; set; }
    
    /// <summary>
    ///     机构名称
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    ///     租户id
    /// </summary>
    public string TenantId { get; set; }
    
    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}