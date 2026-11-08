using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Events;
using Findx.Extensions;
using Findx.Imaging;
using Findx.Module.EleAdminPlus.Shared.Events;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.File;
using Findx.Module.EleAdminPlus.WebAppApi.Options;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.File;
using Findx.NewId;
using Findx.Setting;
using Findx.Storage;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using FileSystemInfo = Findx.Common.FileSystemInfo;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-文件
/// </summary>
[Area("system")]
[Route("api/[area]/file")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-文件"), Description("系统-文件")]
public class SysFileController: QueryControllerBase<SysFileInfo, FileSimplifyDto, FilePageQueryDto, long>
{
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly IRepository<SysFileInfo, long> _repo;
    private readonly IFileStorage _fileStorage;
    private readonly IEventBus _eventBus;
    private readonly IImageProcessor _imageProcessor;
    private readonly ImageOptions _imageOptions;
    private readonly string _folderHost;
    private readonly int _thumbnailWidth;
    private const string FileRoutePrefix = "api/system/file";
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    ///     Ctor
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
        _thumbnailWidth = settingProvider.GetValue<int?>("Findx:Storage:Thumbnail:Width") ?? 200;
    }

    /// <summary>
    ///     文件上传
    /// </summary>
    /// <param name="uploadFileDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload"), Description("上传")]
    [DisableAuditing]
    public async Task<CommonResult> UploadAsync([FromForm] UploadFileDto uploadFileDto, CancellationToken cancellationToken)
    {
        //  组合获取文件对象
        if (uploadFileDto.File == null || uploadFileDto.File.Length < 1)
        {
            if (HttpContext.Request.Form.Files.Any())
            {
                uploadFileDto.File = HttpContext.Request.Form.Files[0];
            }
        }
        
        //  判断是否选择文件
        if (uploadFileDto.File == null || uploadFileDto.File.Length < 1)
            return CommonResult.Fail("4401", "请选择文件!");
        
        //  文件大小检查
        if (uploadFileDto.File.Length > 1024 * 1024 * 50)
            return CommonResult.Fail("4401", "选择的文件已超过50M限制!");
        
        //  组装目录及文件名
        var date = DateTime.Now;
        var name = uploadFileDto.File.FileName;
        var size = uploadFileDto.File.Length;
        var contentType = uploadFileDto.File.ContentType;
        var pathDir = Path.Combine("storage", "default", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
        var id = _keyGenerator.Create();
        var saveName = $"{id.ToString().Replace("-", "")}{Path.GetExtension(name)}"; // 文件名
        var path = Path.Combine(pathDir, saveName);
        var fileInfo = new FileSystemInfo(path, size, name, id.ToString()) { SaveName = saveName };
        
        //  压缩保存
        await using (var fileStream = uploadFileDto.File.OpenReadStream())
        {
            //  图片则按配置缩放/压缩后保存
            await SaveStreamAsync(fileStream, path, fileInfo.Extension, cancellationToken);
            
            //  替换域名
            fileInfo.Url = Path.Combine(_folderHost, fileInfo.Path.SafeString()).NormalizePath();
        }

        var model = new SysFileInfo
        {
            Id = id,
            
            Name = fileInfo.FileName, 
            Length = size / 1024, 
            Path = fileInfo.Path, 
            Url = fileInfo.Url, 
            DownloadUrl = BuildEndpointUrl("download", fileInfo.Path),
            Thumbnail = FileUtility.IsImage(fileInfo.Extension) ? BuildEndpointUrl("thumbnail", fileInfo.Path) : null,
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
    ///     上传 Base64 文件
    /// </summary>
    /// <param name="uploadBase64Dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload/base64"), Description("上传Base64")]
    [DisableAuditing]
    public async Task<CommonResult> UploadBase64Async([FromForm] UploadBase64Dto uploadBase64Dto, CancellationToken cancellationToken)
    {
        if (uploadBase64Dto == null || uploadBase64Dto.Base64.IsNullOrWhiteSpace())
            return CommonResult.Fail("4401", "请选择文件!");

        byte[] bytes;
        string extension;
        string contentType;
        try
        {
            var decoded = DecodeBase64(uploadBase64Dto.Base64, uploadBase64Dto.FileName);
            bytes = decoded.Bytes;
            extension = decoded.Extension;
            contentType = decoded.ContentType;
        }
        catch (FormatException)
        {
            return CommonResult.Fail("4401", "Base64 数据格式错误!");
        }

        if (bytes.Length < 1)
            return CommonResult.Fail("4401", "请选择文件!");

        if (bytes.Length > 1024 * 1024 * 50)
            return CommonResult.Fail("4401", "选择的文件已超过50M限制!");

        //  组装目录及文件名
        var date = DateTime.Now;
        var name = !uploadBase64Dto.FileName.IsNullOrWhiteSpace() ? uploadBase64Dto.FileName : $"base64{extension}";
        var pathDir = Path.Combine("storage", "default", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
        var id = _keyGenerator.Create();
        var saveName = $"{id.ToString().Replace("-", "")}{extension}";
        var path = Path.Combine(pathDir, saveName);
        var fileInfo = new FileSystemInfo(path, bytes.Length, name, id.ToString()) { SaveName = saveName };

        //  保存(图片按配置缩放/压缩)
        await using (var ms = new MemoryStream(bytes))
        {
            await SaveStreamAsync(ms, path, extension, cancellationToken);
            fileInfo.Url = Path.Combine(_folderHost, fileInfo.Path.SafeString()).NormalizePath();
        }

        var model = new SysFileInfo
        {
            Id = id,

            Name = fileInfo.FileName,
            Length = bytes.Length / 1024,
            Path = fileInfo.Path,
            Url = fileInfo.Url,
            DownloadUrl = BuildEndpointUrl("download", fileInfo.Path),
            Thumbnail = FileUtility.IsImage(extension) ? BuildEndpointUrl("thumbnail", fileInfo.Path) : null,
            ContentType = contentType,
            Extension = extension,

            FileType = uploadBase64Dto.FileType,
            FileTypeId = uploadBase64Dto.FileTypeId,
            FileTypeName = uploadBase64Dto.FileTypeName
        };

        model.CheckCreationAudited<SysFileInfo, long>(HttpContext.User);
        model.CheckOrg<SysFileInfo, long>(HttpContext.User);

        await _repo.InsertAsync(model, cancellationToken);

        await _eventBus.PublishAsync(new FileUploadedEvent(model), cancellationToken);

        return CommonResult.Success(fileInfo);
    }

    /// <summary>
    ///     查看原文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("preview/{**path}"), Description("预览")]
    [AllowAnonymous]
    public async Task<IActionResult> PreviewAsync(string path, CancellationToken cancellationToken = default)
    {
        return await StreamFileAsync(path, false, cancellationToken);
    }

    /// <summary>
    ///     下载原文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("download/{**path}"), Description("下载")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        return await StreamFileAsync(path, true, cancellationToken);
    }

    /// <summary>
    ///     查看缩略图
    /// </summary>
    /// <param name="path"></param>
    /// <param name="width">缩略图宽度</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("thumbnail/{**path}"), Description("缩略图")]
    [AllowAnonymous]
    public async Task<IActionResult> ThumbnailAsync(string path, [FromQuery] int? width, CancellationToken cancellationToken = default)
    {
        if (path.IsNullOrWhiteSpace() || path.Contains("..")) return NotFound();

        var extension = Path.GetExtension(path);
        if (!FileUtility.IsImage(extension)) return NotFound();

        await using var source = await _fileStorage.GetFileStreamAsync(path, cancellationToken);
        if (source == null) return NotFound();

        var targetWidth = width is > 0 ? width.Value : _thumbnailWidth;
        await using var resized = await _imageProcessor.ResizeAsync(source, new ImageResizeDto(targetWidth, 0, _imageOptions.ResizeMode), cancellationToken);

        using var buffer = new MemoryStream();
        await resized.CopyToAsync(buffer, cancellationToken);

        return File(buffer.ToArray(), GetContentTypeByExtension(extension));
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

        //  先取出待删除记录的文件路径, 用于清理物理文件
        var paths = await repo.SelectAsync(x => request.Contains(x.Id), x => x.Path, cancellationToken: cancellationToken);

        var total = await repo.DeleteAsync(x => request.Contains(x.Id), cancellationToken);

        //  尽力而为删除物理文件, 失败不影响删除结果
        foreach (var path in paths)
        {
            if (path.IsNullOrWhiteSpace()) continue;
            try
            {
                await _fileStorage.DeleteFileAsync(path, cancellationToken);
            }
            catch
            {
                // ignored
            }
        }

        return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
    }

    /// <summary>
    ///     流式输出文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="attachment">是否作为附件下载</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IActionResult> StreamFileAsync(string path, bool attachment, CancellationToken cancellationToken)
    {
        if (path.IsNullOrWhiteSpace() || path.Contains(".."))
            return NotFound();

        var stream = await _fileStorage.GetFileStreamAsync(path, cancellationToken);
        if (stream == null)
            return NotFound();

        var contentType = GetContentTypeByExtension(Path.GetExtension(path));

        if (!attachment)
            return File(stream, contentType);

        var fileName = Path.GetFileName(path);
        var record = await _repo.FirstAsync(x => x.Path == path, cancellationToken);
        if (record != null && !record.Name.IsNullOrWhiteSpace())
            fileName = record.Name;

        return File(stream, contentType, fileName);
    }

    /// <summary>
    ///     保存数据流到存储(图片按配置缩放/压缩)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="path"></param>
    /// <param name="extension"></param>
    /// <param name="cancellationToken"></param>
    private async Task SaveStreamAsync(Stream source, string path, string extension, CancellationToken cancellationToken)
    {
        if (FileUtility.IsImage(extension) && (_imageOptions.Compress || _imageOptions.ScaleSize))
        {
            using var handle = Pool.MemoryStream.Get(out var ms);

            //  尺寸缩放
            if (_imageOptions.ScaleSize)
            {
                await using var res = await _imageProcessor.ResizeAsync(source, new ImageResizeDto(_imageOptions.ScaleMaxWidth, 0, _imageOptions.ResizeMode), cancellationToken);
                await res.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
            }

            //  质量压缩
            if (_imageOptions.Compress)
            {
                await using var res = await _imageProcessor.CompressAsync(ms.Length > 0 ? ms : source, _imageOptions.CompressQuality, cancellationToken);
                ms.Position = 0;
                ms.SetLength(0);
                await res.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
            }

            await _fileStorage.SaveFileAsync(path, ms, cancellationToken);
        }
        else
        {
            await _fileStorage.SaveFileAsync(path, source, cancellationToken);
        }
    }

    /// <summary>
    ///     构建文件服务接口访问地址
    /// </summary>
    /// <param name="action"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private string BuildEndpointUrl(string action, string path)
    {
        var relative = $"{FileRoutePrefix}/{action}/{path.SafeString()}";
        return Path.Combine(_folderHost, relative).NormalizePath();
    }

    /// <summary>
    ///     依据扩展名推断内容类型
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static string GetContentTypeByExtension(string extension)
    {
        if (!extension.IsNullOrWhiteSpace() && ContentTypeProvider.TryGetContentType("file" + extension, out var mediaType))
            return mediaType;
        return "application/octet-stream";
    }

    /// <summary>
    ///     解析 Base64(兼容 data URI 前缀)
    /// </summary>
    /// <param name="base64"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static (byte[] Bytes, string Extension, string ContentType) DecodeBase64(string base64, string fileName)
    {
        var data = base64;
        string mimeFromUri = null;

        var commaIndex = data.IndexOf(',');
        var head = commaIndex >= 0 ? data[..commaIndex] : data;
        if (head.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var meta = head[5..];
            var semiIndex = meta.IndexOf(';');
            mimeFromUri = semiIndex >= 0 ? meta[..semiIndex] : meta;
        }

        if (commaIndex >= 0)
            data = data[(commaIndex + 1)..];

        var bytes = Convert.FromBase64String(data);

        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (extension.IsNullOrWhiteSpace() && !mimeFromUri.IsNullOrWhiteSpace())
            extension = MapMimeToExtension(mimeFromUri);

        var contentType = !mimeFromUri.IsNullOrWhiteSpace() ? mimeFromUri : GetContentTypeByExtension(extension);

        return (bytes, extension, contentType);
    }

    /// <summary>
    ///     常见图片 MIME 映射为扩展名
    /// </summary>
    /// <param name="mime"></param>
    /// <returns></returns>
    private static string MapMimeToExtension(string mime) => mime.ToLowerInvariant() switch
    {
        "image/png" => ".png",
        "image/jpeg" => ".jpg",
        "image/jpg" => ".jpg",
        "image/gif" => ".gif",
        "image/bmp" => ".bmp",
        "image/webp" => ".webp",
        "image/tiff" => ".tiff",
        _ => string.Empty
    };
}