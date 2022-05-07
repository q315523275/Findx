﻿using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 角色查询入参
    /// </summary>
    public class SysRoleQuery : PageBase
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}