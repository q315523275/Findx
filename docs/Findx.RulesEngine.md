# Findx.RulesEngine

Microsoft RulesEngine 封装，提供规则的校验、注册、执行、查询、移除、清空全流程，并内置一个自定义动作。

> Nuget 包引用

```
Findx.RulesEngine
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `RulesEngine 6.0.1`（NuGet 包名为 `RulesEngine`，源码中通过 `extern alias MicrosoftRulesEngine` 起别名）。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 RulesEngineModule
```

`RulesEngineModule`（`ModuleLevel.Framework`，`Order = 160`）注册：

```csharp
services.AddSingleton<IRulesEngineClient, RulesEngineClient>();
```

核心包已注册 `IRulesEngineFactory`（按名称取多引擎实例）。

## 接口

```csharp
public interface IRulesEngineClient : IServiceNameAware
{
    Response Verify(string ruleRaw);
    Response RegisterRule(string ruleRaw);
    Task<Response> ExecuteAsync<TRequest>(string ruleName, TRequest data, CancellationToken cancellationToken = default);
    bool RuleExists(string ruleName);
    void RemoveRule(string ruleName);
    IEnumerable<string> GetRegisteredRules();
    void ClearAllRules();
}
```

> 返回值说明

`Verify` 与 `RegisterRule` 返回 `Response`（抽象类），成功时无错误列表，失败时 `IsError` 为 `true` 且 `Errors` 携带具体原因。

`ExecuteAsync` 返回 `OkResponse<IEnumerable<RuleResult>>`（内部）或 `ErrorResponse`。

> RuleResult

| 属性 | 说明 |
|---|---|
| `RuleName` | 规则名 |
| `IsSuccess` | 是否命中 |
| `SuccessEvent` | 成功时触发的事件名（字符串，非 C# 事件） |
| `ExceptionMessage` | 失败原因 |
| `ActionResult.Output` | 动作输出值 |
| `ActionResult.Exception` | 动作异常 |

## 规则定义

规则以 JSON 描述（Microsoft RulesEngine 的 Workflow 结构）。

> 规则示例

```json
{
  "WorkflowName": "DiscountRule",
  "Rules": [
    {
      "RuleName": "VipDiscount",
      "Description": "会员等级大于等于2时输出固定折扣",
      "RuleExpressionType": "BinaryExpression",
      "Operator": "And",
      "Conditions": [
        {
          "Condition": {
            "LeftOperand": { "MemberName": "Level", "Operator": "GreaterThanOrEqual", "RightValue": 2 },
            "ComparisonType": "Integer"
          }
        }
      ],
      "Actions": [
        {
          "Name": "DiscountAction",
          "ActionType": "Custom",
          "CustomActionName": "FixedValue",
          "Inputs": { "Value": "VIP-95" }
        }
      ],
      "SuccessEvent": "1",
      "IsSuccess": true,
      "ErrorMessage": "会员等级不足",
      "ActionsOnFailure": []
    }
  ]
}
```

## 自定义动作

`Actions/` 目录用于存放自定义动作（Custom Action），即在 JSON 规则中通过 `ActionType: Custom` + `CustomActionName` 调用的服务端逻辑。

当前内置一个动作：

```csharp
public class FixedValueAction : ActionBase
{
    public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        => ValueTask.FromResult<object>(context.GetContext<string>("Value"));
}
```

注册名为 `"FixedValue"`，行为是从上下文中取出字符串参数 `Value` 并原样作为输出——典型用途是让规则返回固定值而不必编写 C# 代码。

> 自定义动作

```csharp
public class SetDiscountAction : ActionBase
{
    public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
    {
        var rate = context.GetContext<decimal>("Rate");
        return ValueTask.FromResult<object>(rate);
    }
}
```

在自定义模块中注册到引擎设置：

```csharp
var settings = new ReSettings
{
    CustomActions = new Dictionary<string, Func<ActionBase>>
    {
        { "FixedValue", () => new FixedValueAction() },
        { "SetDiscount", () => new SetDiscountAction() }
    }
};
```

## 使用

> 基本流程

```csharp
public class PricingService
{
    private readonly IRulesEngineClient _rules;
    public PricingService(IRulesEngineClient rules) => _rules = rules;

    public async Task<Response> Apply(decimal amount, int level, CancellationToken ct = default)
    {
        // 1. 先校验规则格式
        var verify = _rules.Verify(ruleRaw);
        if (verify.IsError) return verify;

        // 2. 注册（同名会覆盖，可反复注册实现热更新）
        var reg = _rules.RegisterRule(ruleRaw);
        if (reg.IsError) return reg;

        // 3. 执行
        var result = await _rules.ExecuteAsync("DiscountRule", new { Level = level, Amount = amount }, ct);
        if (result is ErrorResponse err) return err;

        foreach (var item in ((OkResponse<IEnumerable<RuleResult>>)result).Data)
            Console.WriteLine($"{item.RuleName}: {item.IsSuccess}, Output={item.ActionResult?.Output}");

        return result;
    }
}
```

输出示例：

```
VipDiscount: True, Output=VIP-95
```

> 规则管理

```csharp
_rules.RuleExists("DiscountRule");     // 判断是否存在
_rules.GetRegisteredRules();           // 列出全部规则名
_rules.RemoveRule("DiscountRule");     // 移除单个
_rules.ClearAllRules();                // 清空全部
```

> 通过工厂获取

```csharp
public class RuleService
{
    private readonly IRulesEngineClient _engine;

    public RuleService(IRulesEngineFactory factory)
        => _engine = factory.Create(nameof(RulesEngineName));
}
```

`IRulesEngineClient` 实现了 `IServiceNameAware`（服务名为 `"Microsoft"`），多引擎场景靠 `Name` 区分。

## 注意事项

- **规则只存在于内存**，`GetRegisteredRules` / `ClearAllRules` 都基于内存集合，应用重启丢失。生产环境需自行在启动时从数据库加载并注册。
- `ExecuteAsync` 的 `TRequest` 是**方法级泛型**，不同请求类型可对同一规则名执行，但规则 JSON 的 `InputType` 必须匹配。
- 返回类型是抽象 `Response`，**拿不到 `Data`**（除强制转换为 `OkResponse<IEnumerable<RuleResult>>` 外），业务结果需从 `RuleResult.ActionResult.Output` 获取。
- `Verify` / `RegisterRule` 是**同步**方法。
- `RuleResult.SuccessEvent` 是字符串事件名，需要外部订阅机制配合才能生效。