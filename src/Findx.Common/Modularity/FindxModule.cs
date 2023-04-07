using Findx.Extensions;

namespace Findx.Modularity
{
    /// <summary>
    /// 模块组件基类
    /// </summary>
    public abstract class FindxModule
    {
        /// <summary>
        /// 获取或设置 模块等级
        /// </summary>
        public virtual ModuleLevel Level => ModuleLevel.Business;

        /// <summary>
        /// 获取 模块启动顺序，模块启动的顺序先按级别启动，同一级别内部再按此顺序启动，
        /// 级别默认为0，表示无依赖，需要在同级别有依赖顺序的时候，再重写为>0的顺序值
        /// </summary>
        public virtual int Order => 0;

        /// <summary>
        /// 获取 是否已可用
        /// </summary>
        public bool IsEnabled { get; protected set; }

        /// <summary>
        /// 将模块服务添加到依赖注入服务容器中
        /// </summary>
        /// <param name="services">依赖注入服务容器</param>
        /// <returns></returns>
        public virtual IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services;
        }

        /// <summary>
        /// 应用模块初始化
        /// </summary>
        /// <param name="provider">服务提供者</param>
        public virtual void UseModule(IServiceProvider provider)
        {
            IsEnabled = true;
        }

        /// <summary>
        /// 应用模块注销
        /// </summary>
        /// <param name="provider"></param>
        public virtual void OnShutdown(IServiceProvider provider)
        {
            IsEnabled = false;
        }

        /// <summary>
        /// 获取当前模块的依赖模块类型
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> GetDependModuleTypes(Type moduleType = null)
        {
            moduleType ??= GetType();
            var dependAttrs = moduleType.GetAttributes<DependsOnModulesAttribute>();
            // ReSharper disable once PossibleMultipleEnumeration
            if (!dependAttrs.Any())
            {
                return Type.EmptyTypes;
            }
            List<Type> dependTypes = new();
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var dependAttr in dependAttrs)
            {
                var packTypes = dependAttr.DependedModuleTypes;
                if (packTypes.Length == 0)
                {
                    continue;
                }
                dependTypes.AddRange(packTypes);
                foreach (var type in packTypes)
                {
                    dependTypes.AddRange(GetDependModuleTypes(type));
                }
            }

            return dependTypes.Distinct();
        }
    }
}
