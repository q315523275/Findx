## 拦截器

使用基于Castle.DynamicProxy组件包扩展实现异步动态拦截的第三方组件包：Castle.Core.AsyncInterceptor

github：https://github.com/JSkimming/Castle.Core.AsyncInterceptor

下面展示rpc组件经常用到的接口方法的拦截代理

> 1、Nuget包引用

Nuget服务查询：Findx.Castle

> 2、定义拦截器


```js
/// <summary>
///     测试方法代理
/// </summary>
public class TestProxyInterceptor : InterceptorBase
{
    private readonly IKeyGenerator<Guid> _keyGenerator;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    public TestProxyInterceptor(IKeyGenerator<Guid> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     InterceptAsync
    /// </summary>
    /// <param name="invocation"></param>
    public override async Task InterceptAsync(IMethodInvocation invocation)
    {
        Console.WriteLine($"befor:{_keyGenerator.Create()},:{invocation.Arguments.ToJson()}");
        invocation.ReturnValue = true;
        Console.WriteLine($"after:{_keyGenerator.Create()}");

        await Task.CompletedTask;
    }
}
```
> 拦截器注册

```js
var testProxyInterceptorType = typeof(TestProxyInterceptor);
var testInterceptorAdapterType = typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(testProxyInterceptorType);
builder.Services.AddTransient(testProxyInterceptorType);
builder.Services.AddTransient(typeof(IMachine), provider =>
{
    var proxyGeneratorInstance = provider.GetRequiredService<IProxyGenerator>();
    return proxyGeneratorInstance.CreateInterfaceProxyWithoutTarget(typeof(IMachine), (IInterceptor)provider.GetRequiredService(testInterceptorAdapterType));
});
```
> 使用

```js
    /// <summary>
    ///     接口动态代理
    /// </summary>
    /// <returns></returns>
    [HttpGet("aspect")]
    public CommonResult Aspect([FromServices] IMachine machine)
    {
        machine.Purchase(1999999);
        return CommonResult.Success();
    }
```

```js
// 控制台打印
before:3a135b83-a6c1-32da-b7f3-66272cfbc8ad,:[1999999]
after:3a135b83-a6cf-b77d-5a1f-67bdb56ce9cc
```
