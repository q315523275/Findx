using Findx.AspNetCore.Extensions;
using Findx.Extensions;
using Findx.Extensions.Scheduler;

var builder = WebApplication.CreateBuilder(args);

//  执行端宿主：SchedulerEnabled=false + WorkerEnabled=true
//  → 容器里没有任何数据库原语，心跳 / 结果 / 日志全部经 ISchedulerMasterChannel 走 HTTP 上报。
//  appsettings.json 里也没有 Findx:FreeSql 数据源，进程不具备访问调度库的能力。
builder.Services.AddFindx()
                .AddModule<SchedulerModule>()
                .AddModules();

//  执行端要接收调度端的 POST /api/scheduler/jobs/execute 与 /kill，所以同样需要 MVC
builder.Services.AddControllers();

var app = builder.Build();

app.UseFindx();

app.MapControllers();

app.Run();
