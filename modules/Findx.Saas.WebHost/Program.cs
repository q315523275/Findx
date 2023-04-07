using System.Text.Encodings.Web;
using System.Text.Json;
using Findx.Admin.WebHost.WebShell;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Serialization;
using Findx.WebSocketCore;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFindx().AddModules();
builder.Services.AddControllers()
                .AddMvcFilter<FindxGlobalAttribute>()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeNullableJsonConverter());
                });
builder.Services.AddCors(options => { options.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true)); });
builder.Services.AddWebSockets(x => { x.KeepAliveInterval = TimeSpan.FromMinutes(2); });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler();
app.UseCors().UseRouting();
app.UseWebSockets().MapWebSocketManager("/ws", app.Services.GetRequiredService<WebSocketHandler>());
app.UseFindx();
app.MapControllersWithAreaRoute();
// app.UseWelcomePage();

// Run Server
app.UseFindxStartup().Run();