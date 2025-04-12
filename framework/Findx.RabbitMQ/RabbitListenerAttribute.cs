using System;

namespace Findx.RabbitMQ;

/// <summary>
///     RabbitMQ监听属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RabbitListenerAttribute : Attribute;