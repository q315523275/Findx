using Findx.AspNetCore.Mvc;
using Findx.Expressions;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.File;

/// <summary>
///     文件查询参数Dto
/// </summary>
public class FileQueryDto: SortCondition
{
    /// <summary>
    ///     关键词
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { set; get; }
    
    /// <summary>
    ///     路径
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.StartsWith)]
    public string Path { set; get; }
    
    /// <summary>
    ///     上传人
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string Creator { set; get; }
    
    /// <summary>
    ///     文件类型
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public int? FileType { set; get; }
    
    /// <summary>
    ///     文件类型Id
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public long? FileTypeId { set; get; }
}