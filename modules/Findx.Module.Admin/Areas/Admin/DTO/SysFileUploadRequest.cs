using Findx.Data;
using Microsoft.AspNetCore.Http;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 系统文件上传
    /// </summary>
    public class SysFileUploadRequest: IRequest
    {
        /// <summary>
        /// 上传信息
        /// </summary>
        public IFormFile file { set; get; }
    }
}
