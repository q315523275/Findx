using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Module.EleAdminPlus.Dtos;
using Findx.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     文件服务
/// </summary>
[Area("system")]
[Route("api/[area]/file")]
[Description("系统-文件")]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-文件")]
public class SysFileController: AreaApiControllerBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyGenerator<Guid> _keyGenerator;
    private readonly IFileStorage _fileStorage;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <param name="applicationContext"></param>
    /// <param name="storageFactory"></param>
    public SysFileController(IKeyGenerator<Guid> keyGenerator, IApplicationContext applicationContext, IStorageFactory storageFactory)
    {
        _keyGenerator = keyGenerator;
        _applicationContext = applicationContext;
        _fileStorage = storageFactory.Create(FileStorageType.Folder.ToString());
    }

    /// <summary>
    /// 文件上传
    /// </summary>
    /// <param name="uploadFileDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("uploadFile")]
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
            return CommonResult.Fail("4401", "选择的文件已超过5M限制!");
        // 组装目录及文件名
        var date = DateTime.Now;
        var name = uploadFileDto.File.FileName;
        var size = uploadFileDto.File.Length;
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
            fileInfo.Url = Path.Combine(_applicationContext.Uris, fileInfo.Path.SafeString()).NormalizePath();
        }
        return CommonResult.Success(fileInfo);
    }
}