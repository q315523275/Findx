using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Module.EleAdmin.Mvc.Filters;
using Findx.SaaS.WebHost.WebShell;
using Findx.Serialization;
using Findx.WebSocketCore.Extensions;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFindx().AddModules();
builder.Services.AddControllers()
       .AddMvcFilter<FindxGlobalAttribute>()
       .AddMvcFilter<EleAdminGlobalAttribute>()
       .AddMvcFilter<AuditOperationAttribute>()
       .AddJsonOptions(options =>
       {
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new DateTimeNullableJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new LongStringJsonConverter());
       });
builder.Services.AddWebSockets(x => { x.KeepAliveInterval = TimeSpan.FromMinutes(2); });
builder.Services.AddCorsAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler().UseCorrelationId().UseCorsAccessor();
app.UseRouting();
app.UseStaticFiles(new StaticFileOptions { RequestPath = "/storage", FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "storage")) });
app.UseFindx();
app.UseWebSockets().MapWebSocket("/ws", app.Services.GetRequiredService<WebSocketHandler>());
app.UseEndpointsWithAreaRoute();

// Run Server
app.UseFindxHosting();
