using System.Text.Encodings.Web;
using System.Text.Unicode;
using Castle.DynamicProxy;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Castle;
using Findx.Data;
using Findx.Extensions;
using Findx.Extensions.AuditLogs.Services;
using Findx.WebHost.Aspect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ReplacePlaceholders();

// Add services to the container.
builder.Services.AddFindx().AddModules();

builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);

builder.Services.AddControllers()
                .AddMvcFilter<FindxGlobalAttribute>()
                .AddJsonOptions(options => { options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });

var rpcProxyInterceptorType = typeof(TestProxyInterceptor);
var rpcInterceptorAdapterType = typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
builder.Services.AddSingleton(new ProxyGenerator());
builder.Services.AddTransient(rpcProxyInterceptorType);
builder.Services.AddTransient(typeof(IMachine), provider =>
    {
        var proxyGeneratorInstance = provider.GetRequiredService<ProxyGenerator>();
        return proxyGeneratorInstance.CreateInterfaceProxyWithoutTarget(typeof(IMachine), (IInterceptor)provider.GetRequiredService(rpcInterceptorAdapterType));
    });

builder.Services.AddScoped<IAuditStore, AuditStoreServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCorrelationId();
app.UseJsonExceptionHandler();
app.UseRouting();
app.UseFindx();
app.MapControllersWithAreaRoute();

app.UseFindxHosting();