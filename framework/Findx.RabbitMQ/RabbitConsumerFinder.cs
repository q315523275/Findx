using System;
using System.Collections.Generic;
using Findx.Reflection;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ消费者查找器
/// </summary>
public class RabbitConsumerFinder : FinderBase<Type>, IRabbitConsumerFinder
{
    protected override IEnumerable<Type> FindAllItems()
    {
        return AssemblyManager.FindTypesByAttribute<RabbitListenerAttribute>();
    }
}