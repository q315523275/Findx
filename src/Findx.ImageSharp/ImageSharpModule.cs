﻿using Findx.Drawing;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Findx.ImageSharp
{
    /// <summary>
    /// Findx-图像处理组件模块
    /// </summary>
    [Description("Findx-图像处理组件模块")]
    public class ImageSharpModule : FindxModule
    {
        /// <summary>
        /// 模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;
        
        /// <summary>
        /// 模块排序
        /// </summary>
        public override int Order => 70;

        /// <summary>
        /// 配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IImageProcessor, ImageProcessor>();
            services.AddSingleton<IVerifyCoder, VerifyCoder>();

            return services;
        }
    }
}
