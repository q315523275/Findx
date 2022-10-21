using System.Text.Encodings.Web;
using System.Text.Unicode;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.WebHost.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFindx().AddModules(typeof(Findx.EventBus.RabbitMQ.EventBusRabbitMqModule)
    , typeof(Findx.Discovery.Consul.ConsulDiscoveryModule)
    , typeof(Findx.Redis.FindxRedisModule)
    , typeof(Findx.FreeSql.FreeSqlModule));
builder.Services.AddHttpClient("policy").AddFallbackPolicy(CommonResult.Fail(), 200).AddCircuitBreakerPolicy(5, "5s").AddRetryPolicy(1).AddTimeoutPolicy(1);
builder.Services.AddControllers().AddMvcFilter<FindxGlobalAttribute>().AddJsonOptions(options => { options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });
// builder.Services.AddHostedService<EventBusWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler();
app.UseFindx();
app.MapControllersWithAreaRoute();

app.UseFindxStartup().Run();
