using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Findx.Builders
{
    /// <summary>
    /// Findx框架构造者
    /// </summary>
    public interface IFindxBuilder
    {
        /// <summary>
        /// 获取 服务集合
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 获取 配置服务
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// 获取 加载的模块集合
        /// </summary>
        IEnumerable<FindxModule> Modules { get; }

        /// <summary>
        /// 添加指定模块
        /// </summary>
        /// <typeparam name="TModuleModule">要添加的模块类型</typeparam>
        IFindxBuilder AddModule<TModuleModule>() where TModuleModule : FindxModule;

        /// <summary>
        /// 添加加载的所有Module，并可排除指定的Module类型
        /// </summary>
        /// <param name="exceptModuleTypes">要排除的Module类型</param>
        /// <returns></returns>
        IFindxBuilder AddModules(params Type[] exceptModuleTypes);
    }
}
