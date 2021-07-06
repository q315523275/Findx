using System;
using System.Collections.Generic;
using System.Text;

namespace Findx.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RabbitListenerAttribute: Attribute
    {
    }
}
