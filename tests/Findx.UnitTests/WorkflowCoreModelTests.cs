using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Options;
using Findx.Extensions.WorkflowCore.Storage;

namespace Findx.UnitTests;

/// <summary>
///     FlowLong 模型解析与条件求值测试
/// </summary>
public class WorkflowCoreModelTests
{
    private const string ValidModel = """
    {
      "processKey": "leave",
      "processName": "请假审批",
      "root": {
        "nodeName": "发起",
        "nodeKey": "root",
        "type": 0,
        "childNode": {
          "nodeName": "主管审批",
          "nodeKey": "leader",
          "type": 1,
          "props": { "assignee": [ { "type": "user", "id": "1", "name": "张主管" } ], "performType": 0 },
          "childNode": {
            "nodeName": "天数判断",
            "nodeKey": "judge",
            "type": 3,
            "childNode": {
              "nodeName": "人事抄送",
              "nodeKey": "cc",
              "type": 2,
              "props": { "assignee": [ { "type": "dept", "id": "d1", "name": "人事部" } ] }
            },
            "conditionNodes": [
              {
                "nodeName": "三天以内",
                "nodeKey": "b1",
                "type": 3,
                "props": { "expression": "days <= 3" },
                "childNode": {
                  "nodeName": "总经理会签",
                  "nodeKey": "cs",
                  "type": 1,
                  "props": { "assignee": [ { "type": "user", "id": "2" }, { "type": "user", "id": "3" } ], "performType": 1 }
                }
              },
              {
                "nodeName": "其它",
                "nodeKey": "b2",
                "type": 3,
                "props": {},
                "childNode": {
                  "nodeName": "人事审批",
                  "nodeKey": "end",
                  "type": 1,
                  "props": { "assignee": [ { "type": "user", "id": "4" } ] }
                }
              }
            ]
          }
        }
      }
    }
    """;

    [Fact]
    public void Parse_ValidModel_BuildsTreeAndMarksBranchOwnership()
    {
        var model = FlwModelParser.Parse(ValidModel, "leave", "请假审批");

        var nodes = model.Root.Traverse().ToList();
        Assert.Equal(8, nodes.Count);

        var countersign = nodes.Single(x => x.NodeKey == "cs");
        Assert.Equal("judge", countersign.BranchParentKey);
        Assert.Equal("b1", countersign.BranchRootKey);
        Assert.Equal(2, countersign.Props.Assignee.Count);
        Assert.Equal(1, (int)countersign.Props.PerformType);

        var cc = nodes.Single(x => x.NodeKey == "cc");
        Assert.Null(cc.BranchParentKey);
    }

    [Fact]
    public void Parse_ConditionBranchWithoutDefault_Throws()
    {
        var json = """
        {
          "root": {
            "nodeName": "发起", "nodeKey": "root", "type": 0,
            "childNode": {
              "nodeName": "条件", "nodeKey": "judge", "type": 3,
              "conditionNodes": [
                { "nodeName": "A", "nodeKey": "a", "type": 3, "props": { "expression": "days > 1" },
                  "childNode": { "nodeName": "审批", "nodeKey": "n1", "type": 1, "props": { "assignee": [ { "type": "user", "id": "1" } ] } } }
              ]
            }
          }
        }
        """;

        var exception = Assert.Throws<FlwModelException>(() => FlwModelParser.Parse(json));
        Assert.Contains("默认分支", exception.Message);
    }

    [Fact]
    public void Parse_DuplicateNodeKey_Throws()
    {
        var json = """
        {
          "root": {
            "nodeName": "发起", "nodeKey": "root", "type": 0,
            "childNode": {
              "nodeName": "A", "nodeKey": "dup", "type": 1,
              "props": { "assignee": [ { "type": "user", "id": "1" } ] },
              "childNode": { "nodeName": "B", "nodeKey": "dup", "type": 1, "props": { "assignee": [ { "type": "user", "id": "2" } ] } }
            }
          }
        }
        """;

        var exception = Assert.Throws<FlwModelException>(() => FlwModelParser.Parse(json));
        Assert.Contains("重复", exception.Message);
    }

    [Fact]
    public void Evaluate_NumericComparison()
    {
        var variables = new Dictionary<string, object> { ["days"] = 3m, ["amount"] = 1200m };

        Assert.True(FlwConditionEvaluator.Evaluate("days <= 3", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("days > 3", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("amount >= 1000 && days == 3", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("${amount} < 2000", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("amount >= 1000 && days > 3", variables));
    }

    [Fact]
    public void Evaluate_ParenthesesAndPrecedence()
    {
        var variables = new Dictionary<string, object> { ["type"] = "leave", ["days"] = 5m };

        Assert.True(FlwConditionEvaluator.Evaluate("(type == 'leave' && days > 3) || false", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("type == 'leave' && (days > 3 || days < 1)", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("!(type == 'leave')", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("!(days < 3)", variables));
    }

    [Fact]
    public void Evaluate_ContainsAndEmptyAndString()
    {
        var variables = new Dictionary<string, object>
        {
            ["reason"] = "年假返乡",
            ["remark"] = "",
            ["flag"] = true
        };

        Assert.True(FlwConditionEvaluator.Evaluate("reason contains '年假'", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("reason contains '病假'", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("reason not contains '年假'", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("empty(remark)", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("notEmpty(reason)", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("flag", variables));
        Assert.False(FlwConditionEvaluator.Evaluate("missing", variables));
        Assert.True(FlwConditionEvaluator.Evaluate("", variables));
    }

    [Fact]
    public void ComputeRemindTime_EdgeCases()
    {
        var now = new DateTime(2026, 11, 11, 9, 0, 0);

        //  未配置时限或提醒天数
        Assert.Null(FlwStore.ComputeRemindTime(now, new FlwNodeProps()));
        Assert.Null(FlwStore.ComputeRemindTime(now, new FlwNodeProps { ExpireDay = 3 }));

        //  提前提醒天数大于期限，提醒点已过，不再提醒
        Assert.Null(FlwStore.ComputeRemindTime(now, new FlwNodeProps { ExpireDay = 3, RemindDay = 5 }));

        //  3 天期限提前 1 天提醒 → 第 2 天
        var remind = FlwStore.ComputeRemindTime(now, new FlwNodeProps { ExpireDay = 3, RemindDay = 1 });
        Assert.Equal(now.AddDays(2), remind);
    }

    [Fact]
    public void Options_DefaultSafe()
    {
        var options = new WorkflowCoreOptions();
        Assert.False(options.TimeoutEnabled);
        Assert.Equal(FlwTimeoutAction.None, options.TimeoutAction);
        Assert.Equal(200, options.ScanBatchSize);
    }
}
