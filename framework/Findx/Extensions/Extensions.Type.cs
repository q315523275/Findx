﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 类型
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     判断当前类型是否可由指定类型派生
    /// </summary>
    public static bool IsDeriveClassFrom<TBaseType>(this Type type, bool canAbstract = false)
    {
        return IsDeriveClassFrom(type, typeof(TBaseType), canAbstract);
    }

    /// <summary>
    ///     判断当前类型是否可由指定类型派生
    /// </summary>
    public static bool IsDeriveClassFrom(this Type type, Type baseType, bool canAbstract = false)
    {
        Check.NotNull(type, nameof(type));
        Check.NotNull(baseType, nameof(baseType));

        return type.IsClass && (canAbstract || !type.IsAbstract) && type.IsBaseOn(baseType);
    }

    /// <summary>
    ///     判断类型是否为Nullable类型
    /// </summary>
    /// <param name="type"> 要处理的类型 </param>
    /// <returns> 是返回True，不是返回False </returns>
    public static bool IsNullableType(this Type type)
    {
        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    ///     由类型的Nullable类型返回实际类型
    /// </summary>
    /// <param name="type"> 要处理的类型对象 </param>
    /// <returns> </returns>
    public static Type GetNonNullableType(this Type type)
    {
        return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
    }

    /// <summary>
    ///     通过类型转换器获取Nullable类型的基础类型
    /// </summary>
    /// <param name="type"> 要处理的类型对象 </param>
    /// <returns> </returns>
    public static Type GetUnNullableType(this Type type)
    {
        if (!IsNullableType(type)) return type;
        
        var nullableConverter = new NullableConverter(type);
        return nullableConverter.UnderlyingType;
    }

    /// <summary>
    ///     获取类型的Description特性描述信息
    /// </summary>
    /// <param name="type">类型对象</param>
    /// <param name="inherit">是否搜索类型的继承链以查找描述特性</param>
    /// <returns>返回Description特性描述信息，如不存在则返回类型的全名</returns>
    public static string GetDescription(this Type type, bool inherit = true)
    {
        var desc = type.GetAttribute<DescriptionAttribute>(inherit);
        if (desc != null) return desc.Description;
        var displayName = type.GetAttribute<DisplayNameAttribute>(inherit);
        if (displayName != null) return displayName.DisplayName;
        var display = type.GetAttribute<DisplayAttribute>(inherit);
        return display != null ? display.Name : type.Name;
    }

    /// <summary>
    ///     获取成员元数据的Description特性描述信息
    /// </summary>
    /// <param name="member">成员元数据对象</param>
    /// <param name="inherit">是否搜索成员的继承链以查找描述特性</param>
    /// <returns>返回Description特性描述信息，如不存在则返回成员的名称</returns>
    public static string GetDescription(this MemberInfo member, bool inherit = true)
    {
        var desc = member.GetAttribute<DescriptionAttribute>(inherit);
        if (desc != null) return desc.Description;
        var displayName = member.GetAttribute<DisplayNameAttribute>(inherit);
        if (displayName != null) return displayName.DisplayName;
        var display = member.GetAttribute<DisplayAttribute>(inherit);
        return display != null ? display.Name : member.Name;
    }

    /// <summary>
    ///     检查指定类型成员中是否存在指定的Attribute特性
    /// </summary>
    /// <typeparam name="T">要检查的Attribute特性类型</typeparam>
    /// <param name="memberInfo">要检查的类型成员</param>
    /// <param name="inherit">是否从继承中查找</param>
    /// <returns>是否存在</returns>
    public static bool HasAttribute<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
    {
        return memberInfo.IsDefined(typeof(T), inherit);
    }

    /// <summary>
    ///     从类型成员获取指定Attribute特性
    /// </summary>
    /// <typeparam name="T">Attribute特性类型</typeparam>
    /// <param name="memberInfo">类型类型成员</param>
    /// <param name="inherit">是否从继承中查找</param>
    /// <returns>存在返回第一个，不存在返回null</returns>
    public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
    {
        var attributes = memberInfo.GetCustomAttributes(typeof(T), inherit);
        return attributes.FirstOrDefault() as T;
    }

    /// <summary>
    ///     从类型成员获取指定Attribute特性
    /// </summary>
    /// <typeparam name="T">Attribute特性类型</typeparam>
    /// <param name="memberInfo">类型类型成员</param>
    /// <param name="inherit">是否从继承中查找</param>
    /// <returns>返回所有指定Attribute特性的数组</returns>
    public static IEnumerable<T> GetAttributes<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
    {
        return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }

    /// <summary>
    ///     判断类型是否为集合类型
    /// </summary>
    /// <param name="type">要处理的类型</param>
    /// <returns>是返回True，不是返回False</returns>
    public static bool IsEnumerable(this Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    ///     判断当前泛型类型是否可由指定类型的实例填充
    /// </summary>
    /// <param name="genericType">泛型类型</param>
    /// <param name="type">指定类型</param>
    /// <returns></returns>
    public static bool IsGenericAssignableFrom(this Type genericType, Type type)
    {
        Check.NotNull(genericType, nameof(genericType));
        Check.NotNull(type, nameof(type));
        if (!genericType.IsGenericType) throw new ArgumentException("该功能只支持泛型类型的调用，非泛型类型可使用 IsAssignableFrom 方法。");

        var allOthers = new List<Type> { type };
        if (genericType.IsInterface) allOthers.AddRange(type.GetInterfaces());

        foreach (var other in allOthers)
        {
            var cur = other;
            while (cur != null)
            {
                if (cur.IsGenericType) cur = cur.GetGenericTypeDefinition();
                if (cur.IsSubclassOf(genericType) || cur == genericType) return true;
                cur = cur.BaseType;
            }
        }

        return false;
    }

    /// <summary>
    ///     方法是否是异步
    /// </summary>
    public static bool IsAsync(this MethodInfo method)
    {
        return method.ReturnType == typeof(Task) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }

    /// <summary>
    ///     返回当前类型是否是指定基类的派生类
    /// </summary>
    /// <param name="type">当前类型</param>
    /// <param name="baseType">要判断的基类型</param>
    /// <returns></returns>
    public static bool IsBaseOn(this Type type, Type baseType)
    {
        return baseType.IsGenericTypeDefinition ? baseType.IsGenericAssignableFrom(type) : baseType.IsAssignableFrom(type);
    }

    /// <summary>
    ///     返回当前类型是否是指定基类的派生类
    /// </summary>
    /// <typeparam name="TBaseType">要判断的基类型</typeparam>
    /// <param name="type">当前类型</param>
    /// <returns></returns>
    public static bool IsBaseOn<TBaseType>(this Type type)
    {
        var baseType = typeof(TBaseType);
        return type.IsBaseOn(baseType);
    }

    /// <summary>
    ///     返回当前方法信息是否是重写方法
    /// </summary>
    /// <param name="method">要判断的方法信息</param>
    /// <returns>是否是重写方法</returns>
    public static bool IsOverridden(this MethodInfo method)
    {
        return method.GetBaseDefinition().DeclaringType != method.DeclaringType;
    }

    /// <summary>
    ///     返回当前属性信息是否为virtual
    /// </summary>
    public static bool IsVirtual(this PropertyInfo property)
    {
        var accessor = property.GetAccessors().FirstOrDefault();
        if (accessor == null) return false;

        return accessor.IsVirtual && !accessor.IsFinal;
    }

    /// <summary>
    ///     获取类型的全名，附带所在类库
    /// </summary>
    public static string GetFullNameWithModule(this Type type)
    {
        return $"{type.FullName},{type.Module.Name.Replace(".dll", "").Replace(".exe", "")}";
    }

    /// <summary>
    ///     获取类型的显示短名称
    /// </summary>
    public static string ShortDisplayName(this Type type)
    {
        return type.DisplayName(false);
    }

    /// <summary>
    ///     获取类型的显示名称
    /// </summary>
    public static string DisplayName(this Type type, bool fullName = true)
    {
        using var psb = Pool.StringBuilder.Get(out var sb);
        ProcessType(sb, type, fullName);
        return sb.ToString();
    }

    /// <summary>
    ///     判断类型是否为基元类型
    /// </summary>
    /// <remarks>基元类型为 Boolean、、Byte、SByte、Int16、Int32UInt16、UInt32Int64、UInt64IntPtr、、UIntPtr、、Char、 Double和 。Single</remarks>
    /// <param name="type"></param>
    /// <param name="includeEnums"></param>
    /// <returns></returns>
    public static bool IsPrimitiveExtendedIncludingNullable(this Type type, bool includeEnums = false)
    {
        if (IsPrimitiveExtended(type, includeEnums)) return true;

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return IsPrimitiveExtended(type.GenericTypeArguments[0], includeEnums);

        return false;
    }

    #region 私有方法

    private static readonly Dictionary<Type, string> BuiltInTypeNames = new()
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(object), "object" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(string), "string" },
        { typeof(uint), "uint" },
        { typeof(ulong), "ulong" },
        { typeof(ushort), "ushort" },
        { typeof(void), "void" }
    };

    private static void ProcessType(StringBuilder builder, Type type, bool fullName)
    {
        if (type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments();
            ProcessGenericType(builder, type, genericArguments, genericArguments.Length, fullName);
        }
        else if (type.IsArray)
        {
            ProcessArrayType(builder, type, fullName);
        }
        else if (BuiltInTypeNames.TryGetValue(type, out var builtInName))
        {
            builder.Append(builtInName);
        }
        else if (!type.IsGenericParameter)
        {
            builder.Append(fullName ? type.FullName : type.Name);
        }
    }

    private static void ProcessArrayType(StringBuilder builder, Type type, bool fullName)
    {
        var innerType = type;
        // ReSharper disable once PossibleNullReferenceException
        while (innerType.IsArray) innerType = innerType.GetElementType();

        ProcessType(builder, innerType, fullName);

        // ReSharper disable once PossibleNullReferenceException
        while (type.IsArray)
        {
            builder.Append('[');
            builder.Append(',', type.GetArrayRank() - 1);
            builder.Append(']');
            type = type.GetElementType();
        }
    }

    private static void ProcessGenericType(StringBuilder builder, Type type, Type[] genericArguments, int length,
        bool fullName)
    {
        // ReSharper disable once PossibleNullReferenceException
        var offset = type.IsNested ? type.DeclaringType.GetGenericArguments().Length : 0;

        if (fullName)
        {
            if (type.IsNested)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                ProcessGenericType(builder, type.DeclaringType, genericArguments, offset, fullName);
                builder.Append('+');
            }
            else
            {
                builder.Append(type.Namespace);
                builder.Append('.');
            }
        }

        var genericPartIndex = type.Name.IndexOf('`');
        if (genericPartIndex <= 0)
        {
            builder.Append(type.Name);
            return;
        }

        builder.Append(type.Name, 0, genericPartIndex);
        builder.Append('<');

        for (var i = offset; i < length; i++)
        {
            ProcessType(builder, genericArguments[i], fullName);
            if (i + 1 == length) continue;

            builder.Append(',');
            if (!genericArguments[i + 1].IsGenericParameter) builder.Append(' ');
        }

        builder.Append('>');
    }

    private static bool IsPrimitiveExtended(Type type, bool includeEnums)
    {
        if (type.GetTypeInfo().IsPrimitive) return true;

        if (includeEnums && type.GetTypeInfo().IsEnum) return true;

        return type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }

    #endregion
}