using Findx.AspNetCore.Extensions;
using Findx.Caching;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;
using Findx.Extensions;
using Findx.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx();

            services.AddControllers();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            app.UseFindx();

            //var cc = app.ApplicationServices.GetRequiredService<IScheduledTaskManager>();
            //for (var i = 0; i < 4999; i++)
            //{
            //    cc.ScheduleAsync<TestScheduledTask>(new { num = i.ToString() }, "*/3 * * * * *").ConfigureAwait(false).GetAwaiter();
            //}
            //var taskStore = app.ApplicationServices.GetRequiredService<IScheduledTaskStore>();
            //while (true)
            //{
            //    var list = taskStore.GetTasksAsync().GetAwaiter().GetResult();
            //    Console.WriteLine($"共有任务{list.Count},并行执行{list.Count(it => it.IsRuning)}");
            //    Thread.Sleep(2000);
            //}

            //IRabbitMqConsumerFactory rabbitMqConsumerFactory = app.ApplicationServices.GetService<IRabbitMqConsumerFactory>();
            //var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration("findx_event_bus", "direct");
            //var queueDeclareConfiguration = new QueueDeclareConfiguration("Findx.Demo", qos: 1) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };
            //IRabbitMqConsumer rabbitMqConsumer = rabbitMqConsumerFactory.Create(exchangeDeclareConfiguration, queueDeclareConfiguration);
            //rabbitMqConsumer.OnMessageReceived((channel, args) => {
            //    Console.WriteLine($"{DateTime.Now}-----{Encoding.UTF8.GetString(args.Body.ToArray())}");
            //    return Task.CompletedTask;
            //});

            //rabbitMqConsumer.StartConsuming();

            //rabbitMqConsumer.Bind("findx_test");

            //IRabbitMqPublisher rabbitMqPublisher = app.ApplicationServices.GetService<IRabbitMqPublisher>();
            //for (int i = 0; i < 10; i++)
            //{
            //    rabbitMqPublisher.Publish($"我是第{i}条消息", "findx_event_bus", "direct", "findx_test");
            //}

            Console.WriteLine($"序列化" + ServiceLocator.GetService<IJsonSerializer>().Serialize(new User { Name = "田亮", Phone = "15155010775" }, false));
            Console.WriteLine($"代理" + ServiceLocator.GetService<IDiscoveryClient>().GetInstancesAsync("com-axon-finance-funderproxy").GetAwaiter().GetResult().ToJson());
            Console.WriteLine($"name" + ServiceLocator.GetService<IDiscoveryClient>().GetServicesAsync().GetAwaiter().GetResult().ToJson());
            Console.WriteLine($"缓存" + ServiceLocator.GetService<ICacheProvider>().Get().Get<object>("ServiceInstances:com-axon-finance-funderproxy").ToJson());
        }
    }

    public class User
    {
        public string Name { set; get; }
        public string Phone { set; get; }
    }
}
