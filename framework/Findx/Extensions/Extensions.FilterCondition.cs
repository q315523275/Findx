using Findx.Data;
using Findx.Expressions;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 动态查询过滤器
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     构建动态过滤条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public static IEnumerable<FilterCondition> BuildFilterCondition<T>(this T req) where T : class
    {
        var reqType = req.GetType();
        var propertyDynamicGetter = new PropertyDynamicGetter<T>();
        
        // 属性标记查询字段
        var queryFields = SingletonDictionary<Type, Dictionary<PropertyInfo, QueryFieldAttribute>>.Instance.GetOrAdd(reqType, () =>
        {
            var fieldDict = new Dictionary<PropertyInfo, QueryFieldAttribute>();
            var queryFieldProperties = reqType.GetProperties().Where(x => x.HasAttribute<QueryFieldAttribute>());
            foreach (var propertyInfo in queryFieldProperties)
            {
                fieldDict[propertyInfo] = propertyInfo.GetAttribute<QueryFieldAttribute>();
            }
            return fieldDict;
        });
        
        // 标记字段计算
        foreach (var fieldInfo in queryFields)
        {
            var objectValue = propertyDynamicGetter.GetPropertyValue(req, fieldInfo.Key.Name);
            string valueString;
            
            if (objectValue is DateTime dateTimeValue)
            {
                valueString = dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                valueString = objectValue?.CastTo<string>();
            }

            if (valueString.IsNotNullOrWhiteSpace())
            {
                yield return new FilterCondition { Field = fieldInfo.Value?.Name?? fieldInfo.Key.Name, Operator = fieldInfo.Value?.FilterOperate?? FilterOperate.Equal, Value = valueString };
            }
        }
    }
}