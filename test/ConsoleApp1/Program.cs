// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Findx.Data;

Console.Title = "Findx 控制台测试";
Console.WriteLine("Hello, World!");

// 一致哈希
// var nodes = new ConsistentHash<string>();
// nodes.Add("192.168.1.101");
// nodes.Add("192.168.1.102");
// nodes.Add("192.168.1.103");
// nodes.Add("192.168.1.104");
// nodes.Add("192.168.1.105");
// nodes.Add("192.168.1.106");
// var dict = new ConcurrentDictionary<string, int>();
// for (int i = 0; i < 100000; i++)
// {
//     var node = nodes.GetItemNode("172.1.0.12"); // 指定固定内容
//     dict.AddOrUpdate(node, 1, (_, value) => value + 1);
// }
// foreach (var item in dict)
// {
//     Console.WriteLine($"{item.Key}:{item.Value}");
// }
// Console.ReadLine();

// 正则
// var a = Findx.Utils.RegexUtil.GetValue("abc4d5e6hh5654", @"\d+");
// Console.WriteLine($"a的值为：{a};");
// var b = Findx.Utils.RegexUtil.GetValues("abc4d5e6hh5654", @"\d+");
// Console.WriteLine($"b的值为：{JsonSerializer.Serialize(b)}");

// 进程执行
// var version = await ProcessX.StartAsync("dotnet --version").FirstAsync();
// Console.WriteLine(version);

// 机器信息
// var network = NetworkInfo.TryGetRealNetworkInfo();
// if (network == null) return;
// var oldRate = network.IpvSpeed();
// var oldRateLength = oldRate.ReceivedLength + oldRate.SendLength;
// var networkSpeed = SizeInfo.Get(network.Speed);
// var v1 = CpuHelper.GetCpuTime();
//
// while (true)
// {
//     Thread.Sleep(1000);
//     
//     var v2 = CpuHelper.GetCpuTime();
//     var value = CpuHelper.CalculateCpuLoad(v1, v2);
//     v1 = v2;
//
//     var memory = MemoryHelper.GetMemoryValue();
//     var newRate = network.IpvSpeed();
//     var nodeRate = SizeInfo.Get(newRate.ReceivedLength + newRate.SendLength - oldRateLength);
//     var speed = NetworkInfo.GetSpeed(oldRate, newRate);
//     oldRate = newRate;
//     
//     Console.Clear();
//     Console.WriteLine($"Cpu:{(int)(value * 100)} %");
//     Console.WriteLine($"已用内存:{memory.UsedPercentage} %");
//     Console.WriteLine($"网卡信息:{network.Name},{network.UnicastAddresses.FirstOrDefault()};Ips:{NetworkInfo.GetIpAddresses().Select(x => x.MapToIPv4().ToString()).ToJson()}");
//     Console.WriteLine($"网卡连接速度:{networkSpeed.Size} {networkSpeed.SizeType}/s");
//     Console.WriteLine($"监测流量:{nodeRate.Size} {nodeRate.SizeType} 上传速率:{speed.Sent.Size} {speed.Sent.SizeType}/s 下载速率:{speed.Received.Size} {speed.Received.SizeType}/s");
// }

// 机器占用端口
// foreach (var activeTcpListener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
// {
//     Console.WriteLine($"{activeTcpListener.Address.MapToIPv4().ToString()}:{activeTcpListener.Port}");
// }

// webSocketClient测试
// var webSocketClient = new XWebSocketClient("ws://127.0.0.1:10021/ws")
// {
//     MessageReceived = (session, message, _) =>
//     {
//         Console.WriteLine(message);
//         return Task.CompletedTask;
//     },
//     OnClosed = () =>
//     {
//         Console.WriteLine("websocket closed");
//         return Task.CompletedTask;
//     },
//     OnError = (message, ex) =>
//     {
//         Console.WriteLine("ex:" + ex.Message);
//         return Task.CompletedTask;
//     }
// };
// webSocketClient.Headers.Add("name", "控制台测试哦");
// webSocketClient.StartAsync().Wait();
// while (true)
// {
//     Console.WriteLine($"请输入websocket发送内容");
//     var msg = Console.ReadLine();
//     webSocketClient.SendAsync(msg).Wait();
// }

// 配置中心测试
// var client = new ConfigClient("1", "2", "dev", "http://localhost:10020;http://localhost:10021");
// client.OnConfigDataChange(x =>
// {
//     Console.WriteLine(x.ToJson());
//     return Task.CompletedTask;
// });
// await client.LoadAsync().ConfigureAwait(false);
// Console.WriteLine("开始配置监听");
// Console.ReadLine();

var user = new User();

user.SetProperty("name", "测试");
Console.WriteLine($"GetProperty:{user.GetProperty<string>("name")}");
Console.WriteLine($"ExtraProperties:{user.ExtraProperties}");


Console.ReadLine();

class User : IExtraObject
{
    public string ExtraProperties { get; set; }
}
