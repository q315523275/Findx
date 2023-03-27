using Findx.AspNetCore.Extensions;
using Findx.Configuration.Extensions;
using Findx.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFindxConfig();

// Add services to the container.
builder.Services.AddFindx().AddModules();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseJsonExceptionHandler();
app.UseFindx();
app.MapControllersWithAreaRoute();

app.Run();