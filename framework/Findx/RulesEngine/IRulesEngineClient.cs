using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Responses;

namespace Findx.RulesEngine;

/// <summary>
///     规则引擎客户端
/// </summary>
public interface IRulesEngineClient: IServiceNameAware
{
    /// <summary>
    ///     检查规则格式是否正确
    /// </summary>
    /// <param name="ruleRaw"></param>
    /// <returns></returns>
    Response Verify(string ruleRaw);

    /// <summary>
    ///     根据参数执行指定规则并返回执行结果
    /// </summary>
    /// <param name="ruleRaw">Rule Json</param>
    /// <param name="data">Input data</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <returns>Whether the match was successful</returns>
    Task<Response> ExecuteAsync<TRequest>(string ruleRaw, TRequest data);
}