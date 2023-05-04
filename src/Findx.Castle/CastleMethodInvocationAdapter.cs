using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Findx.Aspect;

namespace Findx.Castle
{
    public class CastleMethodInvocationAdapter : CastleMethodInvocationAdapterBase, IMethodInvocation
    {
        public CastleMethodInvocationAdapter(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed) : base(invocation)
        {
            ProceedInfo = proceedInfo;
            Proceed = proceed;
        }

        protected IInvocationProceedInfo ProceedInfo { get; }
        protected Func<IInvocation, IInvocationProceedInfo, Task> Proceed { get; }

        public override async Task ProceedAsync()
        {
            await Proceed(Invocation, ProceedInfo);
        }
    }
}