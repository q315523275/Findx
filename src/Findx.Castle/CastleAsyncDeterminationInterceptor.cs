using Castle.DynamicProxy;

namespace Findx.Castle
{
	public class CastleAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor where TInterceptor : Aspect.IInterceptor
	{
		public CastleAsyncDeterminationInterceptor(TInterceptor interceptor): base(new CastleAsyncInterceptorAdapter<TInterceptor>(interceptor))
		{
		}
	}
}

