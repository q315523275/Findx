using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Locks;
using Findx.PerfMonitor;
using Findx.PerfMonitor.MetricsProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;

namespace Findx.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddSingleton<IMetricsProvider, ProcessMetricsProvider>();
            // services.AddSingleton<IMetricsProvider, SystemTotalCpuProvider>();
            // services.AddSingleton<IMetricsProvider, FreeAndTotalMemoryProvider>();

            services.AddFindx().AddModules();

            services.AddControllers(options => options.Filters.Add(typeof(ValidationModelAttribute)))
                   .AddJsonOptions(options => { options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseFindx();

            app.UseMiddleware<AuthTest>();
            
            app.UseAuthentication().UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });

            var provider = app.ApplicationServices;
            //var consumerProvider = provider.GetRequiredService<IRedisMqConsumerProvider>();
            //var publisher = provider.GetRequiredService<IRedisMqPublisher>();


            //var connect = provider.GetRequiredService<IStackExchangeRedisDataBaseProvider>().GetConnection(provider.GetService<IOptions<RedisOptions>>().Value);

            //var database = connect.GetDatabase();

            //var consumer1 = consumerProvider.Create(new QueueConsumerConfiguration($"findx:redis_stream_queue_demo", "group1", "c1"));
            //var consumer2 = consumerProvider.Create(new QueueConsumerConfiguration($"findx:redis_stream_queue_demo", "group1", "c2"));
            //var consumer3 = consumerProvider.Create(new QueueConsumerConfiguration($"findx:redis_stream_queue_demo", "group2", "c3"));

            //consumer1.OnMessageReceived((id, message) => { Console.WriteLine($"consumer1消费消息：{id}==={message}"); return Task.CompletedTask; });
            //consumer2.OnMessageReceived((id, message) => { Console.WriteLine($"consumer2消费消息：{id}==={message}"); return Task.CompletedTask; });
            //consumer3.OnMessageReceived((id, message) => { Console.WriteLine($"consumer3消费消息：{id}==={message}"); return Task.CompletedTask; });

            //consumer1.StartConsuming();
            //consumer2.StartConsuming();
            //consumer3.StartConsuming();

            //database.StreamCreateConsumerGroup($"findx:redis_stream_queue_demo", "group1", "0-0");

            //Task.Factory.StartNew(async () =>
            //{
            //    while (true)
            //    {
            //        var res = await database.StreamReadGroupAsync("findx:redis_stream_queue_demo", "group1", "c1");
            //        if (res != null)
            //        {
            //            foreach (var item in res)
            //            {
            //                Console.WriteLine($"redis_consumer1消费消息：{item.Id}==={item.Values[0].Value}");
            //                database.StreamAcknowledge("findx:redis_stream_queue_demo", "group1", item.Id);
            //            }
            //        }
            //    }
            //});
            //Task.Factory.StartNew(async () =>
            //{
            //    while (true)
            //    {
            //        var res = await database.StreamReadGroupAsync("findx:redis_stream_queue_demo", "group1", "c2");
            //        if (res != null)
            //        {
            //            foreach (var item in res)
            //            {
            //                Console.WriteLine($"redis_consumer2消费消息：{item.Id}==={item.Values[0].Value}");
            //                database.StreamAcknowledge("findx:redis_stream_queue_demo", "group1", item.Id);
            //            }
            //        }
            //    }
            //});

            //for (var i = 0; i < 10; i++)
            //{
            //    publisher.Publish(new { id = i, name = "findx", time = DateTime.Now }, $"findx:redis_stream_queue_demo");
            //}

            // var csredis = new CSRedis.CSRedisClient("10.10.141.128:6379");

            // csredis.Lock

            ////csredis.XGroupCreate($"findx:redis_stream_queue_demo", "group1", "0-0", true);
            ////csredis.XGroupCreate($"findx:redis_stream_queue_demo", "group2", "0-0", false);

            //Task.Factory.StartNew(() =>
            //{

            //    while (true)
            //    {
            //        var res = csredis.XReadGroup("group1", "c3", 1, 5000, ($"findx:redis_stream_queue_demo", ">"));
            //        if (res != null)
            //        {
            //            foreach (var item in res)
            //            {
            //                foreach (var value in item.data)
            //                {
            //                    Console.WriteLine($"csredis_consumer1消费消息：{value.id}==={value.items[1]}");
            //                    csredis.XAck($"findx:redis_stream_queue_demo", "group1", value.id);
            //                    csredis.XDel($"findx:redis_stream_queue_demo", value.id);
            //                }
            //            }
            //        }
            //        Console.WriteLine($"csredis_consumer1消费消息：查看是否阻塞");
            //    }
            //});
            //Task.Factory.StartNew(() =>
            //{

            //    while (true)
            //    {
            //        var res = csredis.XReadGroup("group1", "c2", 1, 5000, ($"findx:redis_stream_queue_demo", "0-0"));
            //        if (res != null)
            //        {
            //            foreach (var item in res)
            //            {
            //                foreach (var value in item.data)
            //                {
            //                    Console.WriteLine($"consumer2消费消息：{value.id}==={value.ToJson()}");
            //                    csredis.XAck($"findx:redis_stream_queue_demo", "group1", value.id);
            //                }
            //            }
            //        }
            //    }
            //});
            //Task.Factory.StartNew(() =>
            //{

            //    while (true)
            //    {
            //        var res = csredis.XReadGroup("group2", "c1", 1, 5000, ($"findx:redis_stream_queue_demo", "0-0"));
            //        if (res != null)
            //        {
            //            foreach (var item in res)
            //            {
            //                foreach (var value in item.data)
            //                {
            //                    Console.WriteLine($"consumer3消费消息：{value.id}==={value.ToJson()}");
            //                    csredis.XAck($"findx:redis_stream_queue_demo", "group1", value.id);
            //                }
            //            }
            //        }
            //    }
            //});

            //for (var i = 0; i < 10; i++)
            //{
            //    // publisher.Publish(new { id = i, name = "findx", time = DateTime.Now }, $"findx:redis_stream_queue_demo");
            //    csredis.XAdd($"findx:redis_stream_queue_demo", ("text", (new { id = i, name = "findx", time = DateTime.Now }).ToJson()));
            //}



            var locker = provider.GetRequiredService<IDistributedLock>();

            var ulock = locker.GetLock("1245", 10);
            ulock.TryLock();
            Thread.Sleep(10 * 1000);
            ulock.UnLock();

        }
    }
}
