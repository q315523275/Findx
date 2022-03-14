using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx.Builders
{
    public class FindxBuilder : IFindxBuilder
    {
        private readonly IEnumerable<FindxModule> _sourceModules;
        private List<FindxModule> _modules;

        public FindxBuilder(IServiceCollection services)
        {
            Services = services;
            Configuration = services.GetConfiguration();
            Check.NotNull(Configuration, nameof(Configuration));
            _sourceModules = GetAllModules(services);
            _modules = new List<FindxModule>();
        }

        public IServiceCollection Services { get; }

        public IConfiguration Configuration { get; }

        public IEnumerable<FindxModule> Modules => _modules;

        public IFindxBuilder AddModule<TModule>() where TModule : FindxModule
        {
            Type type = typeof(TModule);
            return AddModule(type);
        }

        public IFindxBuilder AddModules(params Type[] exceptModuleTypes)
        {
            var source = _sourceModules;
            var exceptModules = source.Where(m => exceptModuleTypes.Contains(m.GetType()));
            source = source.Except(exceptModules);
            foreach (FindxModule module in source)
            {
                AddModule(module.GetType());
            }
            return this;
        }

        private IFindxBuilder AddModule(Type type)
        {
            if (!type.IsBaseOn(typeof(FindxModule)))
            {
                throw new Exception($"要加载的FindxModule型“{type}”不派生于基类 FindxModule");
            }

            if (_modules.Any(m => m.GetType() == type))
            {
                return this;
            }

            var tmpModules = new FindxModule[_modules.Count()];
            _modules.CopyTo(tmpModules);
            var module = _sourceModules.FirstOrDefault(m => m.GetType() == type);
            if (module == null)
            {
                throw new Exception($"类型为“{type.FullName}”的模块实例无法找到");
            }
            _modules.TryAdd(module);

            // 添加依赖模块
            var dependTypes = module.GetDependModuleTypes();
            foreach (var dependType in dependTypes)
            {
                var dependPack = _sourceModules.FirstOrDefault(m => m.GetType() == dependType);
                if (dependPack == null)
                {
                    throw new Exception($"加载模块{module.GetType().FullName}时无法找到依赖模块{dependType.FullName}");
                }
                _modules.TryAdd(dependPack);
            }

            // 按先层级后顺序的规则进行排序
            _modules = _modules.OrderBy(m => m.Level).ThenBy(m => m.Order).ToList();

            var logName = typeof(FindxBuilder).FullName;
            tmpModules = _modules.Except(tmpModules).ToArray();
            foreach (var tmpModule in tmpModules)
            {
                var moduleType = tmpModule.GetType();
                var moduleName = moduleType.GetDescription();
                var tmp = Services;
                AddModule(Services, tmpModule);
                Services.LogInformation($"模块《{moduleName}》的服务添加完毕,添加了 {Services.Count - tmp.Count()} 个服务", logName);
            }

            return this;
        }

        private static IServiceCollection AddModule(IServiceCollection services, FindxModule module)
        {
            Type type = module.GetType();
            Type serviceType = typeof(FindxModule);

            if (type.BaseType?.IsAbstract == false)
            {
                // 移除多重继承的模块
                var descriptors = services.Where(m => m.Lifetime == ServiceLifetime.Singleton && m.ServiceType == serviceType
                                                      && m.ImplementationInstance?.GetType() == type.BaseType);
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
            }

            if (!services.Any(m => m.Lifetime == ServiceLifetime.Singleton && m.ServiceType == serviceType && m.ImplementationInstance?.GetType() == type))
            {
                services.AddSingleton(typeof(FindxModule), module);
                module.ConfigureServices(services);
            }

            return services;
        }

        private static IEnumerable<FindxModule> GetAllModules(IServiceCollection services)
        {
            IFindxModuleTypeFinder moduleTypeFinder = services.GetOrAddTypeFinder<IFindxModuleTypeFinder>(assemblyFinder => new FindxModuleTypeFinder(assemblyFinder));
            var moduleTypes = moduleTypeFinder.FindAll();
            return moduleTypes.Select(m => (FindxModule)Activator.CreateInstance(m))
                              .OrderBy(m => m.Level).ThenBy(m => m.Order).ThenBy(m => m.GetType().FullName);
        }

    }
}
