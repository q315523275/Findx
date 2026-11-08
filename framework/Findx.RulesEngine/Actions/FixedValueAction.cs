extern alias MicrosoftRulesEngine;
using System.Threading.Tasks;
using MicrosoftRulesEngine::RulesEngine.Actions;
using MicrosoftRulesEngine::RulesEngine.Models;

namespace Findx.RulesEngine.Actions;

public class FixedValueAction: ActionBase
{
    public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
    {
        try
        {
            return ValueTask.FromResult<object>(context.GetContext<string>("Value"));
        }
        catch
        {
            //  规则未传入 Value 上下文时返回 null，避免动作直接抛 KeyNotFoundException
            return ValueTask.FromResult<object>(null);
        }
    }
}