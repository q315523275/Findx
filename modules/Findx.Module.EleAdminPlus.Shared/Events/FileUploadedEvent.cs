using Findx.Common;
using Findx.Messaging;
using Findx.Module.EleAdminPlus.Shared.Models;

namespace Findx.Module.EleAdminPlus.Shared.Events;

/// <summary>
///     文件已上传事件
/// </summary>
public sealed class FileUploadedEvent: IApplicationEvent, IAsync
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public FileUploadedEvent() { }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="fileInfo"></param>
    public FileUploadedEvent(SysFileInfo fileInfo)
    {
        FileId = fileInfo.Id;
        FileName = fileInfo.Name;
        FileLength = fileInfo.Length;
        FilePath = fileInfo.Path;
        FileUrl = fileInfo.Url;
        
        FileType = fileInfo.FileType;
        FileTypeId = fileInfo.FileTypeId;
        FileTypeName = fileInfo.FileTypeName;
        
        OrgId = fileInfo.OrgId;
        OrgName = fileInfo.OrgName;
        Creator = fileInfo.Creator;
        CreatorId = fileInfo.CreatorId;
        CreatedTime = fileInfo.CreatedTime;
    }

    /// <summary>
    ///     文件Id
    /// </summary>
    public long FileId { get; set; }
    
    /// <summary>
    ///     文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    ///     文件路径
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    ///     文件大小(kb)
    /// </summary>
    public long FileLength { get; set; }
    
    /// <summary>
    ///     文件网址
    /// </summary>
    public string FileUrl { get; set; }
    
    
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
    ///     创建人
    /// </summary>
    public long? CreatorId { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}