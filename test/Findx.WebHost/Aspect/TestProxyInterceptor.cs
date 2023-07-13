using System;
using System.Threading.Tasks;
using Findx.Aspect;
using Findx.Data;
using Findx.Extensions;

namespace Findx.WebHost.Aspect;

/// <summary>
///     测试方法代理
/// </summary>
public class TestProxyInterceptor : InterceptorBase
{
    private readonly IKeyGenerator<Guid> _keyGenerator;

    public TestProxyInterceptor(IKeyGenerator<Guid> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    public override async Task InterceptAsync(IMethodInvocation invocation)
    {
        Console.WriteLine($"befor:{_keyGenerator.Create()},:{invocation.Arguments.ToJson()}");
        invocation.ReturnValue = true;
        Console.WriteLine($"after:{_keyGenerator.Create()}");

        await Task.CompletedTask;
    }
}