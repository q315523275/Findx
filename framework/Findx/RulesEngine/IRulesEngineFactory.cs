using Findx.DependencyInjection;

namespace Findx.RulesEngine;

/// <summary>
///     规则引擎服务工厂
/// </summary>
public interface IRulesEngineFactory: IServiceFactory<IRulesEngineClient>;