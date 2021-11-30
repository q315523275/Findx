using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Findx.Aspect
{
    public interface IMethodInvocation
    {
        object[] Arguments { get; }

        IReadOnlyDictionary<string, object> ArgumentsDictionary { get; }

        Type[] GenericArguments { get; }

        object TargetObject { get; }

        MethodInfo Method { get; }

        bool IsAsyncMethod { get; }

        object ReturnValue { get; set; }

        Task ProceedAsync();
    }
}
