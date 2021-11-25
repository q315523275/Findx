using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Upload;
using Findx.AspNetCore.Upload.Params;
using Findx.Data;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Internals;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;
using Findx.Linq;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统文件
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysFileInfo")]
    public class SysFileController : CrudControllerBase<SysFileInfo, SysFileInfo, SysFileInfo, SysFileInfo, SysFileQuery, long, long>
    {
        private readonly IApplicationInstanceInfo _instanceInfo;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="instanceInfo"></param>
        public SysFileController(IApplicationInstanceInfo instanceInfo)
        {
            _instanceInfo = instanceInfo;
        }

        protected override Expressionable<SysFileInfo> CreatePageWhereExpression(SysFileQuery request)
        {
            return ExpressionBuilder.Create<SysFileInfo>().AndIF(request.FileLocation > 0, x => x.FileLocation == request.FileLocation)
                                                          .AndIF(!request.FileOriginName.IsNullOrWhiteSpace(), x => x.FileOriginName.Contains(request.FileOriginName))
                                                          .AndIF(!request.FileBucket.IsNullOrWhiteSpace(), x => x.FileBucket == request.FileBucket);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="id">文件编号</param>
        /// <returns></returns>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileInfo([FromQuery, Required] string id)
        {
            var repo = GetRepository<SysFileInfo>();

            var file = await repo.GetAsync(id);
            var fileName = HttpUtility.UrlEncode(file.FileOriginName, Encoding.GetEncoding("UTF-8"));

            switch (file.FileLocation)
            {
                default:
                    var path = Path.Combine(_instanceInfo.RootPath, file.FilePath);
                    return new FileStreamResult(new FileStream(path, FileMode.Open), "application/octet-stream") { FileDownloadName = fileName };
            }
        }

        /// <summary>
        /// 预览文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("preview")]
        public async Task<IActionResult> PreviewFileInfo([FromQuery, Required] string id)
        {
            return await DownloadFileInfo(id);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="request"></param>
        /// <param name="uploadService"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<CommonResult> UploadAsync([FromForm] SysFileUploadRequest request, [FromServices] IFileUploadService uploadService)
        {
            var singleFileUpload = new SingleFileUploadParam
            {
                Request = HttpContext.Request,
                FormFile = request.file,
                RootPath = _instanceInfo.RootPath,
                Module = "storage",
                Group = "default"
            };
            // 本地上传
            var fileInfo = await uploadService.UploadAsync(singleFileUpload);
            // 记录存储
            var sysFileInfo = new SysFileInfo
            {
                FileLocation = (int)FileLocationEnum.LOCAL,
                FileBucket = "default",
                FileOriginName = fileInfo.FileName,
                FilePath = fileInfo.FullPath,
                FileSizeInfo = fileInfo.Size.ToString(),
                FileSizeKb = fileInfo.Size.GetSizeByK().To<long>(),
                FileSuffix = fileInfo.Extension,
                Id = Findx.Utils.SnowflakeId.Default().NextId(),
                FileObjectName = fileInfo.SaveName,
            };
            await base.AddAsync(sysFileInfo);

            return CommonResult.Success(sysFileInfo);
        }



    }
}
