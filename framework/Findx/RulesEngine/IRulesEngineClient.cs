using System.Threading.Tasks;
using Findx.Common;
using Findx.DependencyInjection;

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
    ///     注册一个新的规则到规则引擎中
    /// </summary>
    /// <param name="ruleRaw"></param>
    /// <returns></returns>
    Response RegisterRule(string ruleRaw);
    
    /// <summary>
    ///     根据参数执行指定规则并返回执行结果
    /// </summary>
    /// <param name="ruleName">规则名称</param>
    /// <param name="data">规则参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns>执行结果</returns>
    Task<Response> ExecuteAsync<TRequest>(string ruleName, TRequest data, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     检查给定的规则是否存在。
    /// </summary>
    /// <param name="ruleName">要检查的规则名称。</param>
    /// <returns>如果存在则返回true，否则返回false。</returns>
    bool RuleExists(string ruleName);

    /// <summary>
    ///     移除指定名称的规则。
    /// </summary>
    /// <param name="ruleName">要移除的规则名称。</param>
    /// <returns>是否成功移除了规则。</returns>
    void RemoveRule(string ruleName);

    /// <summary>
    ///     获取所有已注册规则的名称列表。
    /// </summary>
    /// <returns>包含所有规则名称的集合。</returns>
    IEnumerable<string> GetRegisteredRules();

    /// <summary>
    ///     清空所有的规则。
    /// </summary>
    void ClearAllRules();
}