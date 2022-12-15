using System.Text.Encodings.Web;
using System.Text.Unicode;
using Castle.DynamicProxy;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Castle;
using Findx.Data;
using Findx.Extensions;
using Findx.WebHost.Aspect;
using Findx.WebHost.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFindx().AddModules(
                typeof(Findx.RabbitMQ.FindxRabbitMqModule)
                , typeof(Findx.EventBus.RabbitMQ.EventBusRabbitMqModule)
                , typeof(Findx.Discovery.Consul.ConsulDiscoveryModule)
                , typeof(Findx.Redis.FindxRedisModule)
                , typeof(Findx.FreeSql.FreeSqlModule));
builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);
builder.Services.AddControllers()
                .AddMvcFilter<FindxGlobalAttribute>()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                });
// builder.Services.AddHostedService<EventBusWorker>();

var rpcProxyInterceptorType = typeof(TestProxyInterceptor);
var rpcInterceptorAdapterType = typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
builder.Services.AddSingleton(new ProxyGenerator());
builder.Services.AddTransient(rpcProxyInterceptorType);
builder.Services.AddTransient(
    typeof(IMachine),
    serviceProvider =>
    {
        var proxyGeneratorInstance = serviceProvider.GetRequiredService<ProxyGenerator>();
        return proxyGeneratorInstance.CreateInterfaceProxyWithoutTarget(typeof(IMachine), (IInterceptor)serviceProvider.GetRequiredService(rpcInterceptorAdapterType));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler();
app.UseFindx();
app.MapControllersWithAreaRoute();

app.UseFindxStartup().Run();
