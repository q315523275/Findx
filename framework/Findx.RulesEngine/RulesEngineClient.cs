extern alias MicrosoftRulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Responses;
using Findx.Utilities;
using Microsoft.Extensions.Logging;
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
        _rulesEngine = new MicrosoftRulesEngine::RulesEngine.RulesEngine(Array.Empty<Workflow>());
    }

    public Response Verify(string ruleRaw)
    {
        try
        {
            if (ruleRaw.ToObject<Workflow>() == null)
            {
                return new ErrorResponse(new Error("illegal rules", "500"));
            }
            
            return new OkResponse();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "检查规则引擎Json格式异常");
            return new ErrorResponse(new Error(ex.Message, "catch"));
        }
    }

    public async Task<Response> ExecuteAsync<TRequest>(string ruleRaw, TRequest data)
    {
        CheckAndAddRule(ruleRaw, out var workflow);

        var ruleResultTrees = await _rulesEngine.ExecuteAllRulesAsync(workflow!.WorkflowName, data);

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
    
    private void CheckAndAddRule(string ruleRaw, out Workflow workflow)
    {
        _rulesEngine.ClearWorkflows();
        workflow = ruleRaw.ToObject<Workflow>();
        if (workflow == null)
            throw new FindxException("rules.setting.error", "执行规则引擎异常,请检查规则配置");

        if (workflow.WorkflowName.IsNullOrWhiteSpace())
            workflow.WorkflowName = SnowflakeIdUtility.Default().NextId().ToString();
        
        _rulesEngine.AddWorkflow(workflow);
    }
}