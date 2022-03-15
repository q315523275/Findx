using Findx.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 系统配置管理
    /// </summary>
    public class SysConfigQuery : PageBase
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分组编号
        /// </summary>
        public string GroupCode { get; set; }
    }
}
