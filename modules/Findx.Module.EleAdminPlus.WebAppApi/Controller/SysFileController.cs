using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Events;
using Findx.Extensions;
using Findx.Imaging;
using Findx.Module.EleAdminPlus.Shared.Events;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.File;
using Findx.Security;
using Findx.Setting;
using Findx.Storage;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     文件服务
/// </summary>
[Area("system")]
[Route("api/[area]/file")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-文件"), Description("系统-文件")]
public class SysFileController: QueryControllerBase<SysFileInfo, FileDto, FilePageQueryDto, long>
{
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly IFileStorage _fileStorage;
    private readonly IRepository<SysFileInfo, long> _repo;
    private readonly IEventBus _eventBus;
    private readonly string _folderHost;
    private readonly ISettingProvider _settingProvider;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <param name="applicationContext"></param>
    /// <param name="storageFactory"></param>
    /// <param name="repo"></param>
    /// <param name="settingProvider"></param>
    /// <param name="eventBus"></param>
    public SysFileController(IKeyGenerator<long> keyGenerator, IApplicationContext applicationContext, IStorageFactory storageFactory, IRepository<SysFileInfo, long> repo, ISettingProvider settingProvider, IEventBus eventBus)
    {
        _keyGenerator = keyGenerator;
        _applicationContext = applicationContext;
        _repo = repo;
        _settingProvider = settingProvider;
        _eventBus = eventBus;
        _fileStorage = storageFactory.Create(FileStorageType.Folder.ToString());
        _folderHost = settingProvider.GetValue<string>("Findx:Storage:Folder:Host") ?? $"//{HostUtility.ResolveHostAddress(HostUtility.ResolveHostName())}:{_applicationContext.Port}";
    }

    /// <summary>
    /// 文件上传
    /// </summary>
    /// <param name="uploadFileDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("uploadFile"), Description("上传")]
    [DisableAuditing]
    public async Task<CommonResult> UploadFileAsync([FromForm] UploadFileDto uploadFileDto, CancellationToken cancellationToken)
    {
        // 组合获取文件对象
        if (uploadFileDto.File == null || uploadFileDto.File.Length < 1)
        {
            if (HttpContext.Request.Form.Files.Any())
            {
                uploadFileDto.File = HttpContext.Request.Form.Files[0];
            }
        }
        
        // 判断是否选择文件
        if (uploadFileDto.File == null || uploadFileDto.File.Length < 1)
            return CommonResult.Fail("4401", "请选择文件!");
        
        // 文件大小检查
        if (uploadFileDto.File.Length > 1024 * 1024 * 50)
            return CommonResult.Fail("4401", "选择的文件已超过50M限制!");
        
        // 组装目录及文件名
        var date = DateTime.Now;
        var name = uploadFileDto.File.FileName;
        var size = uploadFileDto.File.Length;
        var contentType = uploadFileDto.File.ContentType;
        var pathDir = Path.Combine("storage", "default", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
        var id = _keyGenerator.Create();
        var saveName = $"{id.ToString().Replace("-", "")}{Path.GetExtension(name)}"; // 文件名
        var path = Path.Combine(pathDir, saveName);
        var fileInfo = new FileSpec(path, size, name, id.ToString()) { SaveName = saveName };
        
        // 文件全路径
        var fullPath = Path.Combine(_applicationContext.RootPath, fileInfo.Path.SafeString());
        
        // 压缩保存
        await using (var fileStream = uploadFileDto.File.OpenReadStream())
        {
            await _fileStorage.SaveFileAsync(fullPath, fileStream, cancellationToken); // 内部 Path.Combine 组合
            // 替换域名
            fileInfo.Url = Path.Combine(_folderHost, fileInfo.Path.SafeString()).NormalizePath();
        }

        var model = new SysFileInfo
        {
            Id = id,
            
            Name = fileInfo.FileName, 
            Length = size / 1024, 
            Path = fileInfo.Path, 
            Url = fileInfo.Url, 
            DownloadUrl = fileInfo.Url,
            ContentType = contentType,
            Extension = fileInfo.Extension,
            
            FileType = uploadFileDto.FileType,
            FileTypeId = uploadFileDto.FileTypeId, 
            FileTypeName = uploadFileDto.FileTypeName
        };

        model.CheckCreationAudited<SysFileInfo, long>(HttpContext.User);
        model.CheckOrg<SysFileInfo, long>(HttpContext.User);
        
        await _repo.InsertAsync(model, cancellationToken);

        await _eventBus.PublishAsync(new FileUploadedEvent(model), cancellationToken);
            
        return CommonResult.Success(fileInfo);
    }
    
    /// <summary>
    ///     删除数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    [Description("删除")]
    public virtual async Task<CommonResult> DeleteAsync([FromBody] [MinLength(1)] List<long> request, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysFileInfo, long>();
        var total = await repo.DeleteAsync(x => request.Contains(x.Id), cancellationToken);
        return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
    }
}