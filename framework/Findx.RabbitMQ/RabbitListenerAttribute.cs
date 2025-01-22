using System;

namespace Findx.RabbitMQ;

/// <summary>
///     监听属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RabbitListenerAttribute : Attribute;