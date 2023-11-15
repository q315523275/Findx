// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using ConsoleApp1;
using Findx;
using Findx.Configuration;
using Findx.Extensions;
using Findx.Linq;
using Findx.Utilities;
using MassTransit;

Console.Title = "Findx 控制台测试";
Console.WriteLine("Hello, World!");

// 一致哈希
var nodes = new ConsistentHash<string>();
nodes.Add("192.168.1.101");
nodes.Add("192.168.1.102");
nodes.Add("192.168.1.103");
nodes.Add("192.168.1.104");
nodes.Add("192.168.1.105");
nodes.Add("192.168.1.106");
var dict = new ConcurrentDictionary<string, int>();
for (int i = 0; i < 100000; i++)
{
    var node = nodes.GetItemNode("172.1.0.12"); // 指定固定内容
    dict.AddOrUpdate(node, 1, (_, value) => value + 1);
}
foreach (var item in dict)
{
    Console.WriteLine($"{item.Key}:{item.Value}");
}

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
// var client = new ConfigClient("1", "2", "dev", "http://106.54.160.19:10020", isRecover: true);
// client.OnConfigDataChange(x =>
// {
//     Console.WriteLine(x.ToJson());
//     return Task.CompletedTask;
// });
// await client.LoadAsync().ConfigureAwait(false);
// Console.WriteLine("开始配置监听");
// Console.ReadLine();

// 实体扩展字段
// var user = new User();
// user.SetProperty("name", "测试");
// user.SetProperty("obj", new Test { Title = "这是遥远的信息" });
// user.SetProperty("number", 9.88);
// Console.WriteLine($"ExtraProperties:{user.ExtraProperties}");
// Console.WriteLine($"GetProperty:{user.GetProperty<string>("name")}");
// Console.WriteLine($"GetProperty:{user.GetProperty<Test>("obj")?.Title}");
// Console.WriteLine($"GetProperty:{user.GetProperty<decimal>("number")}");
//
//
// Console.ReadLine();
//
// internal class User : IExtraObject
// {
//     public string ExtraProperties { get; set; }
// }
//
// public class Test
// {
//     public string Title { get; set; }
// }

// Json
Console.WriteLine(new { Name = "测试员", Title = "Json测试" }.ToJson());

// Rsa + Aes
// var privateKeyStr = XC.RSAUtil.RsaKeyConvert.PrivateKeyPkcs8ToXml("MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAI0NF4GBZ07BejfYZAmhTk0rofIoQqgRf63erVeHRAKfnEPHtTZ/MMfRupgc1uo6/e7DZ0BqOp58SLeXfcYEzWzzos0ThR797eiSA0AY8tB5L8WOy5tOxfhCiAfM87CpNvqyEyYtFwihVYrIHJdPO/Ll0ugoPVhP/MwIEwVHMFA5AgMBAAECgYAUXDNsajVzVNJDhWTLTxFyaj3yKoWUpRH9EwuKeugCSO/RiN5Lg4iTD18T3fXX0bQd5u7ciXj0r5P/jEqHbuIIBB69+xykWSdD/9O8/s2wX43u5+C7UqkhS0N+SEOYAn6j24l/Wd0RRNTMXMrWudHdZ+FI1Yn2gB8Zf/rZTJ6CUQJBAOf6nlHpGJ9m01BbISUzCKmDZMHrvbQgVoLUzi8zPNpld3n+4vyuN5vy3w/aiWp6uYmGOgfnuVaaBEh5WvkUncMCQQCbqCAzhU7YUGNTaoHRy7WzW+KlRDsJQunUWofGniGX3cnz2RUYCv3IfuP22GuCBOX26MAgeapkSfK/KGf3FY5TAkEAyooimNmvydz5OvuV4OjB817pJfcx1oc1gV1T+BoAU56rxjQo8v0ZSGuxHiJsQC+Otuge2rATPe2TN8PdDgRWCQJAAtQoWadXinjThUWPPGfOUocd9FDsHbv4keJfS02+YIsoS2Uri/dPK2Ca9fZy5bb/EuCh9TUg0pfBcJXkZcoffwJALjLX9RAK4UEYz+YDEaGO7dhSLd1QJbBT8H2ekhxEItTwCSp70JmDx8xltjGfgdzhGQjAWY8iEkp2wrOg8WFu6g==");
// var publicKeyStr = XC.RSAUtil.RsaKeyConvert.PublicKeyPemToXml("MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC/q3kMS8MWnPhFugPSYa/9UiBBQ1KP+cpSO6NtvBPRiBpic9iRNPAcM2sl8wAUIplX2uWHUq3ENQkWkAQaEs5MJ8hZA168OS7Qn+exkLsxR5EjREHGbeIzH1iG5ekHn0ymdc9/bwXba2bzOIaHTNfZT1SuyUdBixYMWshZlVqzuwIDAQAB");
//
// Console.WriteLine($"privateKeyStr:{privateKeyStr}");
// Console.WriteLine($"publicKeyStr:{publicKeyStr}");
//
//
// var ci = "EoXxXstYtcpCUTr8BxKBqZlQD+hHg7q8diBCElz/iCRe4u8jZD8vN9T0lapknNHPEG7WUXZpgavN2qU4RgKxb1xfjWxe3cdRXzGH4O9cJmIQxOy6PZX5F6Tv0qgr9dKZ7UncCIwh127S+6UgFpGcbt71KOwE5Scc1wbo8VFjlBI=";
// var sa = "hK0b1fIiuFX8jxE3FNitAANZ0K0lxDUjnhQucsVpi6cyF13SdpvdhNvTHrS05WS/YFTh5JP4cAwAUnKyz5MEVAvizhb51UGe44x0ieJHfBJ6z+NWLLdDR8gvoUuED7/gLn5ejDdjFqZUuW07EUcZGmj5hvcQGMxliWAoMY3DmF8=";
// var sign = "";
// var data = "RUWevIIJ+HC4uLq5HfDb7yRMuaXw2RIvVfyYgy92hyxxJAReVEQynlBT0CDF4AfIz3F6F5LbwP0Uke5LNdoUvTmRhjcv4Rcd3YL6D0wxzmmTu7VLXka9hu6n9notC7Sgk2yARle5ZvIxR78lMU+o6u7cV/YqeW1dkts9/f51gw0=";
//
// var aesKey = Findx.Utils.Encrypt.RsaDecrypt(ci, privateKeyStr);
// Console.WriteLine(aesKey);
//
// var aesIv = Findx.Utils.Encrypt.RsaDecrypt(sa, privateKeyStr);
// Console.WriteLine(aesIv);
//
// var toEncryptArray = Convert.FromBase64String(data);
//
// var des = Aes.Create();
// des.Key = Encoding.UTF8.GetBytes(aesKey);
// des.Mode = CipherMode.CBC;
// des.IV = Encoding.UTF8.GetBytes(aesIv);
// // des.Padding = PaddingMode.;
//
// var cTransform = des.CreateDecryptor();
// var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
// Console.WriteLine(Encoding.Default.GetString(resultArray));

// 有序Id
// SequentialGuid.Next(Findx.Utils.SequentialGuidType.AsString);
// GuidHelper.Next(SequentialGuidType.AsString);
// Guid.NewGuid();
// SnowflakeId.Default().NextId();
// SequentialGuid.Next(Findx.Utils.SequentialGuidType.AsString);
// Console.WriteLine("Guid生成预热结束");
//
// var watch = new Stopwatch();  
// watch.Start();
// for (int i = 0; i < 1000000; i++)
// {
//     Guid.NewGuid();
// }
// watch.Stop();
// Console.WriteLine($"原生NewGuid耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (int i = 0; i < 1000000; i++)
// {
//     GuidHelper.Next(SequentialGuidType.AsString);
// }
// watch.Stop();
// Console.WriteLine($"纳秒级别有序Guid耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (int i = 0; i < 1000000; i++)
// {
//     NewId.NextSequentialGuid();
// }
// watch.Stop();
// Console.WriteLine($"NewId有序Guid耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (int i = 0; i < 1000000; i++)
// {
//     SequentialGuid.Next(Findx.Utils.SequentialGuidType.AsString);
// }
// watch.Stop();
// Console.WriteLine($"Abp有序Guid耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (int i = 0; i < 1000000; i++)
// {
//     SnowflakeId.Default().NextId();
// }
// watch.Stop();
// Console.WriteLine($"SnowflakeId耗时:{watch.Elapsed.TotalMilliseconds}ms");

// var sequentialGuidList = new HashSet<string>();
// for (int i = 0; i < 1000000; i++)
// {
//     sequentialGuidList.Add(NewId.NextSequentialGuid().ToString());
// }
// Console.WriteLine($"有序guid是否有重复:{sequentialGuidList.Count != 1000000}");

// var sequentialGuidList = new List<Guid>();
// for (int i = 0; i < 100; i++)
// {
//     sequentialGuidList.Add(NewId.NextSequentialGuid());
// }
//
// var newGuids = sequentialGuidList.OrderBy(x => x).ToList();
// for (int i = 0; i < 100; i++)
// {
//     Console.WriteLine($"比对:{sequentialGuidList[i] == newGuids[i]},源:{sequentialGuidList[i]},new:{newGuids[i]}");
// }

// // Json表达式解析
// var dynamicFilter = new DynamicFilterInfo
// {
//     Logic = FilterOperate.And,
//     Filters = new List<FilterConditions>
//     {
//         new()
//         {
//             Field = "Name", Value = "Name110", Operator = FilterOperate.NotContains
//         },
//         new ()
//         {
//             Field = "Status", Value = "0,1", Operator = FilterOperate.In
//         },
//         new()
//         {
//             Field = "CreatedTime", Value = "2021-12-30", Operator = FilterOperate.GreaterOrEqual
//         }
//     }
// };
// var orderConditions = new List<OrderConditions> { new() { Field = "CreatedTime", SortDirection = ListSortDirection.Descending }, new() { Field = "Status", SortDirection = ListSortDirection.Descending } };
// var dataSort = DataSortBuilder.New<SysAppInfo>().OrderBy("Status").OrderBy(x => new { x.CreatedTime, x.Id}).Build();
//
// var filter = LinqExpressionParser.ParseConditions<SysAppInfo>(dynamicFilter);
//
// var entities = new List<SysAppInfo>();
// for (var i = 0; i < 1000; i++)
// {
//     entities.Add(new SysAppInfo
//     {
//         Id = SequentialGuidUtility.Next(SequentialGuidType.AsString),
//         Name = "Name" + (i + 1),
//         Code = "Code" + (i + 1),
//         CreatedTime = DateTime.Now,
//         Status = i,
//     });
// }
//
// var s = entities.Where(filter.Compile()).OrderBy(dataSort.First().Conditions.Compile());  //.OrderConditions(orderConditions);
//
// Console.WriteLine($"{entities.Count}---{s.Count()}");

// Console.ReadLine();

// var moveFile = "/Users/tianliang/Downloads/生产流程图.jpg";
// var moveToFile = "/Users/tianliang/Downloads/生产流程图_222.jpg";
// var moveToFile2 = "/Users/tianliang/Downloads/生产流程图_333.jpg";
// FileUtility.Copy(moveFile, moveToFile, true);
// FileUtility.Move(moveToFile, moveToFile2);
// Console.WriteLine(FileUtility.GetCreationTime(moveFile));
// Console.WriteLine(FileUtility.GetLastAccessTime(moveFile));
// Console.WriteLine(FileUtility.GetEncoding(moveFile).EncodingName);
// Console.WriteLine(FileUtility.GetFileMd5(moveFile));

// var movePath = "/Users/tianliang/Downloads/psi-master";
// var moveToPath = "/Users/tianliang/Downloads/psi-master2";

// DirectoryUtility.Move(movePath, moveToPath);
// DirectoryUtility.Copy(movePath, moveToPath);
// Console.WriteLine(DirectoryUtility.GetFiles(movePath, "*").Select(x => x.Name).ToJson());
// Console.WriteLine(DirectoryUtility.EnumerateFiles(movePath, "*").ToJson());
// Console.WriteLine(DirectoryUtility.GetDirectories(movePath, "*").ToJson());

Console.WriteLine($"11111");











