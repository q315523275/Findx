using Findx.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFindxConfig();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var s = app.Configuration.GetValue<string>("2");

Console.WriteLine($"获取配置中心key:2,值:{s}");

app.Run();