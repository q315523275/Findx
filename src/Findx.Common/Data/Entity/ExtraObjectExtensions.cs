using System.Text.Json.Nodes;
using Findx.Extensions;
using Findx.Serialization;

namespace Findx.Data;

/// <summary>
/// 实体扩展对象对应扩展方法
/// </summary>
public static class ExtraObjectExtensions
{
    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="extraObject"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetProperty<T>(this IExtraObject extraObject, string name)
    {
        extraObject.ThrowIfNull(nameof(extraObject));
        name.ThrowIfNull(nameof(name));

        if (string.IsNullOrWhiteSpace(extraObject.ExtraProperties)) 
            return default;

        var jsonObject = JsonNode.Parse(extraObject.ExtraProperties)!.AsObject();
        
        if (!jsonObject.ContainsKey(name))
            return default;
        
        if (typeof(T).IsPrimitiveExtendedIncludingNullable(true))
            return jsonObject[name]!.GetValue<T>();

        return jsonObject[name].SafeString().ToObject<T>();
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="extraObject"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public static void SetProperty<T>(this IExtraObject extraObject, string name, T value)
    {
        extraObject.ThrowIfNull(nameof(extraObject));

        if (string.IsNullOrWhiteSpace(extraObject.ExtraProperties))
        {
            if (EqualityComparer<T>.Default.Equals(value, default)) return;
    
            extraObject.ExtraProperties = "{}";
        }
    
        var jsonObject = JsonNode.Parse(extraObject.ExtraProperties)?.AsObject();

        jsonObject.ThrowIfNull(nameof(jsonObject));
    
        if (value == null || EqualityComparer<T>.Default.Equals(value, default))
        {
            // ReSharper disable once PossibleNullReferenceException
            if (jsonObject[name] != null) 
                jsonObject.Remove(name);
        }
        else if (value.GetType().IsPrimitiveExtendedIncludingNullable(true))
        {
            // ReSharper disable once PossibleNullReferenceException
            jsonObject[name] = JsonValue.Create(value);
        }
        else
        {
            // ReSharper disable once PossibleNullReferenceException
            jsonObject[name] = value.ToJson();
        }
        
        // ReSharper disable once PossibleNullReferenceException
        var data = jsonObject.ToJsonString(SystemTextJsonStringSerializer.Options);
        if (data == "{}")
        {
            data = null;
        }
    
        extraObject.ExtraProperties = data;
    }

    /// <summary>
    /// 移除属性值
    /// </summary>
    /// <param name="extraObject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool RemoveProperty(this IExtraObject extraObject, string name)
    {
        extraObject.ThrowIfNull(nameof(extraObject));
        name.ThrowIfNull(nameof(name));
    
        if (string.IsNullOrWhiteSpace(extraObject.ExtraProperties))
        {
            return false;
        }
    
        var jsonObject = JsonNode.Parse(extraObject.ExtraProperties)?.AsObject();
        
        jsonObject.ThrowIfNull(nameof(jsonObject));
    
        // ReSharper disable once PossibleNullReferenceException
        var token = jsonObject[name];
        if (token == null)
            return false;
    
        jsonObject.Remove(name);
    
        var data = jsonObject.ToJsonString(SystemTextJsonStringSerializer.Options);
        if (data == "{}")
        {
            data = null;
        }
    
        extraObject.ExtraProperties = data;
    
        return true;
    }
}