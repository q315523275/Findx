extern alias MicrosoftRulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;
using Findx.RulesEngine.Actions;
using Microsoft.Extensions.Logging;
using MicrosoftRulesEngine::RulesEngine.Actions;
using MicrosoftRulesEngine::RulesEngine.Models;

namespace Findx.RulesEngine;

public class RulesEngineClient: IRulesEngineClient
{
    public string Name => "Microsoft";
    
    private readonly MicrosoftRulesEngine::RulesEngine.RulesEngine _rulesEngine;
    
    private readonly ILogger<RulesEngineClient> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    public RulesEngineClient(ILogger<RulesEngineClient> logger)
    {
        _logger = logger;
        var settings = new ReSettings
        {
            CustomActions = new Dictionary<string, Func<ActionBase>>
            {
                { "FixedValue", () => new FixedValueAction() }
            }
        };
        _rulesEngine = new MicrosoftRulesEngine::RulesEngine.RulesEngine(Array.Empty<Workflow>(), settings);
    }

    public Response Verify(string ruleRaw)
    {
        try
        {
            var workflow = ruleRaw.ToObject<Workflow>();
            if (workflow == null)
                return new ErrorResponse(new Error("rules.setting.error", "规则Json数据格式不匹配"));

            if (workflow.WorkflowName.IsNullOrWhiteSpace())
                return new ErrorResponse(new Error("rules.setting.error", "规则Json数据缺失规则名称"));
            
            return new OkResponse();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "检查规则引擎Json格式异常");
            return new ErrorResponse(new Error(ex.Message, "规则引擎Json格式序列化异常"));
        }
    }

    public Response RegisterRule(string ruleRaw)
    {
        var workflow = ruleRaw.ToObject<Workflow>();
        if (workflow == null)
            return new ErrorResponse(new Error("rules.setting.error", "规则Json数据格式不匹配"));

        if (workflow.WorkflowName.IsNullOrWhiteSpace())
            return new ErrorResponse(new Error("rules.setting.error", "规则Json数据缺失规则名称"));

        _rulesEngine.AddOrUpdateWorkflow(workflow);

        return new OkResponse();
    }

    public async Task<Response> ExecuteAsync<TRequest>(string ruleName, TRequest data, CancellationToken cancellationToken = default)
    {
        var ruleResultTrees = await _rulesEngine.ExecuteAllRulesAsync(ruleName, data);

        var ruleResult = ruleResultTrees.Select(x => new RuleResult
        {
            RuleName = x.Rule.RuleName,
            IsSuccess = x.IsSuccess,
            SuccessEvent = x.Rule.SuccessEvent,
            ExceptionMessage = x.ExceptionMessage,
            ActionResult = new RuleActionResult { Output = x.ActionResult?.Output, Exception = x.ActionResult?.Exception }
        });
        
        return new OkResponse<IEnumerable<RuleResult>>(ruleResult);
    }

    public bool RuleExists(string ruleName)
    {
        return _rulesEngine.ContainsWorkflow(ruleName);
    }

    public void RemoveRule(string ruleName)
    {
        _rulesEngine.RemoveWorkflow(ruleName);
    }

    public IEnumerable<string> GetRegisteredRules()
    {
        return _rulesEngine.GetAllRegisteredWorkflowNames();
    }

    public void ClearAllRules()
    {
        _rulesEngine.ClearWorkflows();
    }
}