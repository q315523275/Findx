using Findx.AspNetCore.Extensions;
using Findx.Extensions;
using Findx.Extensions.Scheduler;
using Findx.Samples.SchedulerServer;

var builder = WebApplication.CreateBuilder(args);

//  AddFindx().AddModules() 会自动发现并加载 SchedulerModule；这里显式 AddModule<SchedulerModule>()
//  是为了让示例一眼能看出「这个宿主装了哪个组件」，两者同时存在不会重复加载。
//  模块内部的角色拆分由 Findx:Scheduler:SchedulerEnabled / WorkerEnabled 决定：
//  本示例只开调度端，因此容器里只有数据库原语与 Master 服务，没有执行队列。
builder.Services.AddFindx()
                .AddModule<SchedulerModule>()
                .AddModules();

//  调度端的管理面与机器间接口（心跳/结果/日志/投递回收）都是 MVC 控制器
builder.Services.AddControllers();

var app = builder.Build();

//  初始化 Findx 模块（FreeSql AOP、模块 UseModule 等）
app.UseFindx();

app.MapControllers();

//  演示用：确保 scheduler_jobs 里有三条示例任务定义（已存在则跳过）
await DemoJobSeeder.SeedAsync(app.Services, app.Logger);

app.Run();
