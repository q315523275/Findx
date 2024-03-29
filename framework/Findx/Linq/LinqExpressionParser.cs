using System.Linq.Expressions;
using Findx.Extensions;

namespace Findx.Linq;

/// <summary>
///    Linq表达式解析器
/// </summary>
public static class LinqExpressionParser
{
    /// <summary>
    ///     解析条件
    /// </summary>
    /// <returns></returns>
    public static Expression<Func<T, bool>> ParseConditions<T>(DynamicFilterInfo dynamicFilterInfo)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        
        var bodies = new List<Expression>();
        bodies.AddRange(dynamicFilterInfo.Filters.Select(x => ParseExpressionBody(parameter, x)));

        var body = dynamicFilterInfo.Logic switch
        {
            FilterOperate.And => bodies.Aggregate(Expression.AndAlso),
            FilterOperate.Or => bodies.Aggregate(Expression.OrElse),
            _ => Expression.Constant(true)
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    ///     解析条件
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    private static Expression ParseExpressionBody(ParameterExpression parameter, FilterConditions condition)
    {
        if (condition == null) 
            return Expression.Constant(true);
        
        var left = GetPropertyExpression(parameter, condition);
        if (left == null) 
            return Expression.Constant(true);
        
        switch (condition.Operator)
        {
            case FilterOperate.Contains:
            case FilterOperate.NotContains:
                if (left.Type != typeof(string))
                    throw new NotSupportedException("“NotContains”比较方式只支持字符串类型的数据");
                var methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) }) ?? throw new InvalidOperationException($"名称为“Contains”的方法不存在");
                var exp = Expression.Call(left, methodInfo, ChangeTypeToExpression(condition.Value, left.Type));
                if (condition.Operator == FilterOperate.NotContains)
                    return Expression.Not(exp);
                return exp;
            
            case FilterOperate.StartsWith:
                if (left.Type != typeof(string))
                    throw new NotSupportedException("“StartsWith”比较方式只支持字符串类型的数据");
                var methodInfoStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) }) ?? throw new InvalidOperationException($"名称为“StartsWith”的方法不存在");
                return Expression.Call(left, methodInfoStartsWith, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.EndsWith:
                if (left.Type != typeof(string))
                    throw new NotSupportedException("“EndsWith”比较方式只支持字符串类型的数据");
                var methodInfoEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) }) ?? throw new InvalidOperationException($"名称为“EndsWith”的方法不存在");
                return Expression.Call(left, methodInfoEndsWith, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.Equal:
                return Expression.Equal(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.NotEqual:
                return Expression.NotEqual(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.Greater:
                return Expression.GreaterThan(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.GreaterOrEqual:
                return Expression.GreaterThanOrEqual(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.Less:
                return Expression.LessThan(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.LessOrEqual:
                return Expression.LessThanOrEqual(left, ChangeTypeToExpression(condition.Value, left.Type));
            
            case FilterOperate.In:
            case FilterOperate.NotIn:
                return condition.Operator == FilterOperate.NotIn ? Expression.Not(CreateExpressionIn(left, condition)) : CreateExpressionIn(left, condition);

            case FilterOperate.Between:
                return CreateExpressionBetween(left, condition);

            case FilterOperate.And:
            case FilterOperate.Or:
            default:
                throw new NotImplementedException("不支持此操作");
        }
    }
    
    /// <summary>
    ///     创建Between解析表达式
    /// </summary>
    /// <param name="left">Expression.Property</param>
    /// <param name="conditions"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    private static Expression CreateExpressionBetween(Expression left, FilterConditions conditions)
    {
        var valueArr = conditions.Value.Split(',');
        if (valueArr.Length != 2)
            throw new InvalidDataException("ParseBetween参数错误");
        
        Expression start = Expression.GreaterThanOrEqual(left, ChangeTypeToExpression(valueArr[0], left.Type));
        Expression end = Expression.LessThanOrEqual(left, ChangeTypeToExpression(valueArr[1], left.Type));
        
        return Expression.AndAlso(start, end);
    }
    
    /// <summary>
    ///     创建In解析表达式
    /// </summary>
    /// <param name="left"></param>
    /// <param name="conditions"></param>
    /// <returns></returns>
    private static Expression CreateExpressionIn(Expression left, FilterConditions conditions)
    {
        var values = conditions.Value.Split(',');
        
        object objectValue;
        if (left.Type == typeof(string))
            objectValue = values.Select(x => x.CastTo<string>());
        else if (left.Type == typeof(int))
            objectValue = values.Select(x => x.CastTo<int>());
        else if (left.Type == typeof(long))
            objectValue = values.Select(x => x.CastTo<long>());
        else if  (left.Type == typeof(decimal))
            objectValue = values.Select(x => x.CastTo<decimal>());
        else if (left.Type == typeof(double))
            objectValue = values.Select(x => x.CastTo<double>());
        else if (left.Type == typeof(float))
            objectValue = values.Select(x => x.CastTo<float>());
        else if (left.Type == typeof(bool))
            objectValue = values.Select(x => x.CastTo<bool>());
        else if (left.Type == typeof(Guid))
            objectValue = values.Select(x => x.CastTo<Guid>());
        else if (left.Type == typeof(DateTime))
            objectValue = values.Select(x => x.CastTo<DateTime>());
        else
            throw new NotSupportedException($"“ParseIn”不支持此类型{left.Type.Name}的数据");
        
        var method = typeof(Enumerable).GetMethods().First(a => a.Name == "Contains").MakeGenericMethod(left.Type);
        var constantCollection = Expression.Constant(objectValue);
        
        return Expression.Call(method, constantCollection, left);
    }
    
    /// <summary>
    ///     获取属性表达式
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="conditions"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Expression GetPropertyExpression(ParameterExpression parameter, FilterConditions conditions)
    {
        // 嵌套字段名，如：User.Name
        var propertyNames = conditions.Field.Split('.');
        Expression propertyAccess = parameter;
        var type = parameter.Type;
        for (var index = 0; index < propertyNames.Length; index++)
        {
            var propertyName = propertyNames[index];
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"指定的属性“{conditions.Field}”在类型“{type.FullName}”中不存在。");
            }

            // 子对象类型
            type = property.PropertyType;
            // 验证最后一个属性与属性值是否匹配
            if (index == propertyNames.Length - 1)
            {
                var flag = CheckFilterConditions(type, conditions);
                if (!flag)
                {
                    return null;
                }
            }

            propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
        }
        return propertyAccess;
    }
    
    /// <summary>
    ///     验证最后一个属性与属性值是否匹配 
    /// </summary>
    /// <param name="type">最后一个属性</param>
    /// <param name="conditions">条件信息</param>
    /// <returns></returns>
    private static bool CheckFilterConditions(Type type, FilterConditions conditions)
    { 
        if (conditions.Value.IsNullOrWhiteSpace() && (type == typeof(string) || type.IsNullableType()))
        {
            return conditions.Operator is FilterOperate.Equal or FilterOperate.NotEqual;
        }

        if (conditions.Value.IsNullOrWhiteSpace())
        {
            return !type.IsValueType;
        }
        
        return true;
    }
    
    /// <summary>
    ///     将指定类型值更改为值表达式
    /// </summary>
    /// <param name="fieldValue"></param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    private static Expression ChangeTypeToExpression(object fieldValue, Type conversionType)
    {
        var targetValue = fieldValue.CastTo(conversionType);
        
        return Expression.Constant(targetValue, conversionType);
    }
}