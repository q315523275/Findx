using Castle.DynamicProxy;

namespace Findx.Castle
{
	public class CastleAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor where TInterceptor : Aspect.IInterceptor
	{
		public CastleAsyncDeterminationInterceptor(TInterceptor abpInterceptor): base(new CastleAsyncInterceptorAdapter<TInterceptor>(abpInterceptor))
		{
		}
	}
}

