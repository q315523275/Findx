using Findx.Expressions;

namespace Findx.UnitTests;

/// <summary>
///     LambdaExpressionParser 回归测试（修复：缓存引用旧 ParameterExpression 导致二次查询编译失败；空 Filters 抛异常）
/// </summary>
public class ExpressionParserTests
{
    private sealed class TestEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void ParseConditions_SameFieldTwice_BothCompile()
    {
        var condition = new FilterCondition { Field = nameof(TestEntity.Name), Value = "a", Operator = FilterOperate.Equal };

        // 第一次构建（缓存了 PropertyInfo）
        var first = LambdaExpressionParser.ParseConditions<TestEntity>(new FilterGroup { Filters = new[] { condition } });
        // 第二次构建同一字段：修复前 body 引用第一次的 ParameterExpression，Compile 抛 InvalidOperationException
        var second = LambdaExpressionParser.ParseConditions<TestEntity>(new FilterGroup { Filters = new[] { condition } });

        var f1 = first.Compile();
        var f2 = second.Compile();

        Assert.False(f1(new TestEntity { Name = "b" }));
        Assert.True(f2(new TestEntity { Name = "a" }));
        Assert.True(f1(new TestEntity { Name = "a" }));
        Assert.False(f2(new TestEntity { Name = "b" }));
    }

    [Fact]
    public void ParseConditions_EmptyFilters_ReturnsConstantTrue()
    {
        // 修复前：bodies 为空时 Aggregate 抛 "Sequence contains no elements"
        var expression = LambdaExpressionParser.ParseConditions<TestEntity>(new FilterGroup { Filters = Array.Empty<FilterCondition>() });

        var func = expression.Compile();
        Assert.True(func(new TestEntity()));
    }

    [Fact]
    public void ParseConditions_IntEqual_Works()
    {
        var condition = new FilterCondition { Field = nameof(TestEntity.Age), Value = "30", Operator = FilterOperate.Equal };
        var expression = LambdaExpressionParser.ParseConditions<TestEntity>(new FilterGroup { Filters = new[] { condition } });

        var func = expression.Compile();
        Assert.True(func(new TestEntity { Age = 30 }));
        Assert.False(func(new TestEntity { Age = 31 }));
    }
}
