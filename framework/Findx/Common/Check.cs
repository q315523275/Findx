﻿using System.Diagnostics;

namespace Findx.Common;

/// <summary>
///     检查类
/// </summary>
[DebuggerStepThrough]
public static class Check
{
    /// <summary>
    ///     不能为NULL检查
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public static T NotNull<T>(T value, string parameterName)
    {
        if (value == null) throw new ArgumentNullException(parameterName);
        return value;
    }

    /// <summary>
    ///     不能为NULL检查
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static T NotNull<T>(T value, string parameterName, string message)
    {
        if (value == null) throw new ArgumentNullException(parameterName, message);
        return value;
    }

    /// <summary>
    ///     不能为NULL检查
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <param name="maxLength"></param>
    /// <param name="minLength"></param>
    /// <returns></returns>
    public static string NotNull(string value, string parameterName, int maxLength = int.MaxValue, int minLength = 0)
    {
        if (value == null) throw new ArgumentException($"{parameterName} can not be null!", parameterName);
        if (value.Length > maxLength)
            throw new ArgumentException($"{parameterName} length must be equal to or lower than {maxLength}!",
                parameterName);
        if (minLength > 0 && value.Length < minLength)
            throw new ArgumentException($"{parameterName} length must be equal to or bigger than {minLength}!",
                parameterName);
        return value;
    }

    /// <summary>
    ///     不能为NULL和空检查
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <param name="maxLength"></param>
    /// <param name="minLength"></param>
    /// <returns></returns>
    public static string NotNullOrWhiteSpace(string value, string parameterName, int maxLength = int.MaxValue,
        int minLength = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName);
        if (value.Length > maxLength)
            throw new ArgumentException($"{parameterName} length must be equal to or lower than {maxLength}!",
                parameterName);
        if (minLength > 0 && value.Length < minLength)
            throw new ArgumentException($"{parameterName} length must be equal to or bigger than {minLength}!",
                parameterName);
        return value;
    }

    /// <summary>
    ///     检查对象是否为NULL 为NULL时抛出异常
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ThrowIfNull<T>(this T data, string name) where T : class
    {
        if (data == null) throw new ArgumentNullException(name);
    }

    /// <summary>
    ///     检查对象是否为NULL 为NULL时抛出异常
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ThrowIfNull<T>(this T data) where T : class
    {
        if (data == null) throw new ArgumentNullException();
    }
}