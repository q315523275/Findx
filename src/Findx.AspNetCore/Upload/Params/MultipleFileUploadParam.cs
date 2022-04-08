﻿using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

namespace Findx.AspNetCore.Upload.Params
{
    /// <summary>
    /// 多文件上传参数
    /// </summary>
    public class MultipleFileUploadParam
    {
        /// <summary>
        /// 当前请求
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// 上传的文件对象
        /// </summary>
        public IEnumerable<IFormFile> FormFiles { get; set; }

        /// <summary>
        /// 存储根路径
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 完整目录
        /// </summary>
        public string FullPath => Path.Combine(RootPath, Module, Group);

        /// <summary>
        /// 相对目录
        /// </summary>
        public string RelativePath => Path.Combine(Module, Group);
    }
}
