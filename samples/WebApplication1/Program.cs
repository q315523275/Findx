using Findx.AspNetCore.Extensions;
using Findx.Configuration.Extensions;
using Findx.Extensions;
using Findx.WebSocketCore.Extensions;
using Microsoft.AspNetCore.WebSockets;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFindxConfig();

// Add services to the container.
builder.Services.AddFindx().AddModules();
builder.Services.AddControllers();
builder.Services.AddWebSockets(x => { x.KeepAliveInterval = TimeSpan.FromMinutes(2); });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler();
app.UseWebSockets().MapWebSocket("/ws", app.Services.GetRequiredService<WebSocketHandler>());
app.UseFindx();
app.MapControllersWithAreaRoute();

app.UseFindxHosting();
