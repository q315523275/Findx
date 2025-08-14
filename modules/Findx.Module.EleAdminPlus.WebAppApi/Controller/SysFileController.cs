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
using Findx.Module.EleAdminPlus.WebAppApi.Options;
using Findx.NewId;
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
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly IRepository<SysFileInfo, long> _repo;
    private readonly IFileStorage _fileStorage;
    private readonly IEventBus _eventBus;
    private readonly IImageProcessor _imageProcessor;
    private readonly ImageOptions _imageOptions;
    private readonly string _folderHost;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <param name="applicationContext"></param>
    /// <param name="storageFactory"></param>
    /// <param name="repo"></param>
    /// <param name="settingProvider"></param>
    /// <param name="eventBus"></param>
    /// <param name="imageProcessor"></param>
    public SysFileController(IKeyGenerator<long> keyGenerator, IApplicationContext applicationContext, IStorageFactory storageFactory, IRepository<SysFileInfo, long> repo, ISettingProvider settingProvider, IEventBus eventBus, IImageProcessor imageProcessor)
    {
        _keyGenerator = keyGenerator;
        _repo = repo;
        _eventBus = eventBus;
        _imageProcessor = imageProcessor;
        _fileStorage = storageFactory.Create(nameof(FileStorageType.Folder));
        _folderHost = settingProvider.GetValue<string>("Findx:Storage:Folder:Host") 
                            ?? $"//{HostUtility.ResolveHostAddress(HostUtility.ResolveHostName())}:{applicationContext.Port}";
        _imageOptions = settingProvider.GetObject<ImageOptions>("Findx:Storage:Image");
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
        // var fullPath = Path.Combine(_applicationContext.RootPath, fileInfo.Path.SafeString());
        
        // 压缩保存
        await using (var fileStream = uploadFileDto.File.OpenReadStream())
        {
            // 图片上传并启用压缩
            if (FileUtility.IsImage(fileInfo.Extension) && (_imageOptions.Compress || _imageOptions.ScaleSize))
            {
                using var ms = Pool.MemoryStream.Rent();

                // 尺寸缩放
                if (_imageOptions.ScaleSize)
                {
                    await using var res = await _imageProcessor.ResizeAsync(fileStream, new ImageResizeDto(_imageOptions.ScaleMaxWidth, 0), cancellationToken);
                    await res.CopyToAsync(ms, cancellationToken);
                    ms.Position = 0;
                }

                // 质量压缩
                if (_imageOptions.Compress)
                {
                    await using var res = await _imageProcessor.CompressAsync(ms.Length > 0 ? ms : fileStream, _imageOptions.CompressQuality, cancellationToken);
                    await res.CopyToAsync(ms, cancellationToken);
                    ms.Position = 0;
                }

                await _fileStorage.SaveFileAsync(path, ms, cancellationToken); // 内部 Path.Combine 组合
            }
            else
            {
                await _fileStorage.SaveFileAsync(path, fileStream, cancellationToken); // 内部 Path.Combine 组合
            }
            
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