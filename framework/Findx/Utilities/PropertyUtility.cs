using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Findx.Utilities;

/// <summary>
///     属性字段工具类
/// </summary>
public static class PropertyUtility
{
    /// <summary>
    ///     表达式获取对象的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public static Func<T, object> ExpressionGetter<T>(string propertyName)
    {
        var type = typeof(T);
        var property = type.GetProperty(propertyName);
        if (property == null)
            throw new Exception($"该类型没有名为{propertyName}的属性");
        
        var getMethod = property.GetGetMethod();
        if (getMethod == null)
            throw new Exception($"该类型{propertyName}属性不支持Getter");
        
        // 对象实例
        var parameterExpression = Expression.Parameter(typeof(object), "obj");
        // 转换参数为真实类型,a.xx
        var unaryExpression = Expression.Convert(parameterExpression, type);
        // 调用获取属性的方法
        var callMethod = Expression.Call(unaryExpression, getMethod);
        var expression = Expression.Lambda<Func<T, object>>(callMethod, parameterExpression);
        
        return expression.Compile();
    }
    
    /// <summary>
    ///     表达式设置对象的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Action<T, object> ExpressionSetter<T>(string propertyName)
    {
        var type = typeof(T);
        var property = type.GetProperty(propertyName);
        if (property == null)
            throw new Exception($"该类型没有名为{propertyName}的属性");
 
        var setMethod = property.GetSetMethod();
        if (setMethod == null)
            throw new Exception($"该类型{propertyName}属性不支持Setter");
 
        var objectParameterExpression = Expression.Parameter(typeof(object), "obj");
        var objectUnaryExpression = Expression.Convert(objectParameterExpression, type);
        
        var valueParameterExpression = Expression.Parameter(typeof(object), "val");
        var valueUnaryExpression = Expression.Convert(valueParameterExpression, property.PropertyType);
 
        // 调用给属性赋值的方法
        var body = Expression.Call(objectUnaryExpression, setMethod, valueUnaryExpression);
        var expression = Expression.Lambda<Action<T, object>>(body, objectParameterExpression, valueParameterExpression);
        
        return expression.Compile();
    }
    
    /// <summary>
    ///     Emit获取对象的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Func<T, object> EmitGetter<T>(string propertyName)
    {
        var type = typeof(T);

        var dynamicMethod = new DynamicMethod("get_" + propertyName, typeof(object), [type], type);
        var iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        
        var property = type.GetProperty(propertyName);
        if (property == null)
            throw new Exception($"该类型没有名为{propertyName}的属性");
        
        var getMethod = property.GetGetMethod();
        if (getMethod == null)
            throw new Exception($"该类型{propertyName}属性不支持Getter");
        
        iLGenerator.Emit(OpCodes.Callvirt, getMethod);
        // 引用类型-转换
        // 值类型-装箱
        iLGenerator.Emit(property.PropertyType.IsValueType ? OpCodes.Box : OpCodes.Castclass, property.PropertyType);
        iLGenerator.Emit(OpCodes.Ret);

        return dynamicMethod.CreateDelegate(typeof(Func<T, object>)) as Func<T, object>;
    }
    
    /// <summary>
    /// Emit设置对象的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Action<T, object> EmitSetter<T>(string propertyName)
    {
        var type = typeof(T);

        var dynamicMethod = new DynamicMethod("EmitCallable", null, [type, typeof(object)], type.Module);
        var iLGenerator = dynamicMethod.GetILGenerator();
        
        var setMethod = type.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
        if (setMethod == null)
            throw new Exception($"该类型{propertyName}属性不支持Setter");
        
        var parameterInfo = setMethod.GetParameters()[0];
        var local = iLGenerator.DeclareLocal(parameterInfo.ParameterType, true);

        iLGenerator.Emit(OpCodes.Ldarg_1);
        // 引用类型-转换
        // 值类型-拆箱
        iLGenerator.Emit(parameterInfo.ParameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterInfo.ParameterType);
        
        iLGenerator.Emit(OpCodes.Stloc, local);
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.Emit(OpCodes.Ldloc, local);
        
        iLGenerator.EmitCall(OpCodes.Callvirt, setMethod, null);
        iLGenerator.Emit(OpCodes.Ret);
        
        return dynamicMethod.CreateDelegate(typeof(Action<T, object>)) as Action<T, object>;
    }
}