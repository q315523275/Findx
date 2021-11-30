using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Aspect
{
    public abstract class InterceptorBase: IInterceptor
    {
        public abstract Task InterceptAsync(IMethodInvocation invocation);
    }
}
