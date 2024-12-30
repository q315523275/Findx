extern alias MicrosoftRulesEngine;
using System.Threading.Tasks;
using MicrosoftRulesEngine::RulesEngine.Actions;
using MicrosoftRulesEngine::RulesEngine.Models;

namespace Findx.RulesEngine.Actions;

public class FixedValueAction: ActionBase
{
    public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
    {
        return ValueTask.FromResult<object>(context.GetContext<string>("Value"));
    }
}