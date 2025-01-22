using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Castle.DynamicProxy;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Castle;
using Findx.Data;
using Findx.Extensions;
using Findx.Serialization;
using Findx.WebHost.Aspect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ReplacePlaceholders();

// Add services to the container.
builder.Services.AddFindx().AddModules();
builder.Services.AddCorsAccessor();

builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);

builder.Services.AddControllers()
    .AddMvcFilter<FindxGlobalAttribute>()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeNullableJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new LongStringJsonConverter());
    });

#region ProxyGenerator
var testProxyInterceptorType = typeof(TestProxyInterceptor);
var testInterceptorAdapterType = typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(testProxyInterceptorType);
builder.Services.AddTransient(testProxyInterceptorType);
builder.Services.AddTransient(typeof(IMachine), provider =>
{
    var proxyGeneratorInstance = provider.GetRequiredService<IProxyGenerator>();
    return proxyGeneratorInstance.CreateInterfaceProxyWithoutTarget(typeof(IMachine), (IInterceptor)provider.GetRequiredService(testInterceptorAdapterType));
});
#endregion

// builder.Services.AddScoped<IAuditStore, AuditStoreServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCorrelationId();
app.UseJsonExceptionHandler();
app.UseCorsAccessor().UseRouting();
app.UseFindx();
app.MapControllersWithAreaRoute();

app.UseFindxHosting();