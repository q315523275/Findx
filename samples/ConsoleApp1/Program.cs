// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleApp1;
using CsvHelper;
using Findx;
using Findx.Configuration;
using Findx.Configuration.Extensions;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Machine;
using Findx.Machine.Cpu;
using Findx.Machine.Memory;
using Findx.Machine.Network;
using Findx.Reflection;
using Findx.Utilities;
using Findx.WebSocketCore;
using Findx.WebSocketCore.Extensions;
using Findx.WebSocketCore.Hubs.Client;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLife.Data;
using Yitter.IdGenerator;

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
// for (var i = 0; i < 100000; i++)
// {
//     var node = nodes.GetItemNode("127.0.0.1"); // 指定固定内容
//     dict.AddOrUpdate(node, 1, (_, value) => value + 1);
// }
// foreach (var item in dict)
// {
//     Console.WriteLine($"{item.Key}:{item.Value}");
// }


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

// #region Host构建
// var builder = Host.CreateApplicationBuilder(args);
// builder.Configuration.AddFindxConfig();
// builder.Services.AddFindx(); 
// var app = builder.Build();
// #endregion
//
// #region Host启动
// await app.UseFindx().RunAsync();
// #endregion

// webSocketClient测试
var hubConnection = new HubConnectionBuilder().WithUrl("ws://127.0.0.1:10021/ws?userName=开发").WithAutomaticReconnection().Build();
await hubConnection.StartAsync();
hubConnection.On(async (message, token) =>  
{
    var txt = await message.AsTextAsync(token);
    Console.WriteLine($"Received {txt}");
});
hubConnection.Closed += (error) =>
{
    Console.WriteLine(error?.Message);
    return Task.FromResult(Task.CompletedTask);
};
while (true)
{
    Console.WriteLine($"请输入websocket发送内容;exit则退出");
    var msg = Console.ReadLine();
    if (msg == "exit")
    {
        await hubConnection.StopAsync();
    }
    else
    {
        await hubConnection.SendAsync(new RequestTextMessage(msg), WebSocketMessageType.Text, true);
    }
}


// 配置中心测试
// var client = new ConfigClient("1", "2", "dev", "http://localhost:9010", isRecovery: true);
// client.OnConfigDataChange(x =>
// {
//     Console.WriteLine(x.ToJson());
//     return Task.CompletedTask;
// });
// await client.LoadAsync().ConfigureAwait(false);
// Console.WriteLine("开始配置监听");
// Console.ReadLine();


// var sign = EncryptUtility.Md5By32($"findxtestdev0");
// Console.WriteLine($"/api/config?appId=findx&sign={sign}&environment=dev&version=0&load=false");


// 实体扩展字段
// var user = new User();
// user.SetProperty("name", "测试");
// user.SetProperty("obj", new Test { Title = "这是遥远的信息" });
// user.SetProperty("number", 9.88);
// Console.WriteLine($"ExtraProperties:{user.ExtraProperties}");
// Console.WriteLine($"GetProperty:{user.GetProperty<string>("name")}");
// Console.WriteLine($"GetProperty:{user.GetProperty<Test>("obj")?.Title}");
// Console.WriteLine($"GetProperty:{user.GetProperty<decimal>("number")}");
// internal class User : IExtraObject
// {
//     public string ExtraProperties { get; set; }
// }
//
// public class Test
// {
//     public string Title { get; set; }
// }


// Csv测试及性能对比
// var userList = new List<User>();
// for (var i = 0; i < 100_000; i++)
// {
//     userList.Add(new User
//     {
//         Id = i + 1,
//         Name = "测试员" + (i + 1), 
//         Title = "Csv测试"
//     });
// }
// var csvPath = Path.Combine(AppContext.BaseDirectory, "test.csv");
// var csvPath2 = Path.Combine(AppContext.BaseDirectory, "test2.csv");
// // 预热
// await CsvUtility.ExportCsvAsync(userList, csvPath, rewrite: true);
// _ = CsvUtility.ReadCsv<User>(csvPath);
//
// await using var writer0 = new StreamWriter(csvPath2);
// await using var csvWriter0 = new CsvWriter(writer0, CultureInfo.InvariantCulture);
// await csvWriter0.WriteRecordsAsync(userList);
// using var reader0 = new StreamReader(csvPath2);
// using var csvReader0 = new CsvReader(reader0, CultureInfo.InvariantCulture);
// csvReader0.GetRecords<User>();
//
// Console.WriteLine($"组建预热完成,开始进行CsvUtility与CsvHelper组建性能比较...");
// Console.WriteLine();
//
// var stopWatch = new Stopwatch();
// stopWatch.Start(); 
// await CsvUtility.ExportCsvAsync(userList, csvPath, rewrite: true);
// stopWatch.Stop();
// Console.WriteLine($"CsvUtility.ExportCsv导出{userList.Count}条记录集合耗时:{stopWatch.ElapsedMilliseconds}ms");
//
// stopWatch.Restart();
// var users = CsvUtility.ReadCsv<User>(csvPath).ToList();
// stopWatch.Stop();
// Console.WriteLine($"CsvUtility.ReadCsv读取{users.Count}条记录集合耗时:{stopWatch.ElapsedMilliseconds}ms");
// Console.WriteLine($"CsvUtility.ReadCsv最后一条json数据:{users.OrderByDescending(x => x.Id).First().ToJson()}");
//
// Console.WriteLine();
//
// // 写入CSV文件数据
// stopWatch.Restart();
// await using var writer = new StreamWriter(csvPath2);
// await using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
// await csvWriter.WriteRecordsAsync(userList);
// stopWatch.Stop();
// Console.WriteLine($"CsvHelper.CsvWriter导出{userList.Count}条记录集合耗时:{stopWatch.ElapsedMilliseconds}ms");
//
// //读取CSV文件数据
// stopWatch.Restart();
// using var reader = new StreamReader(csvPath2);
// using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
// var getStudentInfos = csvReader.GetRecords<User>().ToList();
// stopWatch.Stop();
// Console.WriteLine($"CsvHelper.CsvReader读取{getStudentInfos.Count}条记录集合耗时:{stopWatch.ElapsedMilliseconds}ms");
// Console.WriteLine($"CsvHelper.CsvReader最后一条json数据:{getStudentInfos.OrderByDescending(x => x.Id).First().ToJson()}");
//
// Console.ReadLine();
// // Json
// class User
// {
//     public int Id { set; get; }
//     public string Name { set; get; }
//     public string Title { set; get; }
// }


// Rsa + Aes
// var privateKeyStr = XC.RSAUtil.RsaKeyConvert.PrivateKeyPkcs8ToXml("MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAI0NF4GBZ07BejfYZAmhTk0rofIoQqgRf63erVeHRAKfnEPHtTZ/MMfRupgc1uo6/e7DZ0BqOp58SLeXfcYEzWzzos0ThR797eiSA0AY8tB5L8WOy5tOxfhCiAfM87CpNvqyEyYtFwihVYrIHJdPO/Ll0ugoPVhP/MwIEwVHMFA5AgMBAAECgYAUXDNsajVzVNJDhWTLTxFyaj3yKoWUpRH9EwuKeugCSO/RiN5Lg4iTD18T3fXX0bQd5u7ciXj0r5P/jEqHbuIIBB69+xykWSdD/9O8/s2wX43u5+C7UqkhS0N+SEOYAn6j24l/Wd0RRNTMXMrWudHdZ+FI1Yn2gB8Zf/rZTJ6CUQJBAOf6nlHpGJ9m01BbISUzCKmDZMHrvbQgVoLUzi8zPNpld3n+4vyuN5vy3w/aiWp6uYmGOgfnuVaaBEh5WvkUncMCQQCbqCAzhU7YUGNTaoHRy7WzW+KlRDsJQunUWofGniGX3cnz2RUYCv3IfuP22GuCBOX26MAgeapkSfK/KGf3FY5TAkEAyooimNmvydz5OvuV4OjB817pJfcx1oc1gV1T+BoAU56rxjQo8v0ZSGuxHiJsQC+Otuge2rATPe2TN8PdDgRWCQJAAtQoWadXinjThUWPPGfOUocd9FDsHbv4keJfS02+YIsoS2Uri/dPK2Ca9fZy5bb/EuCh9TUg0pfBcJXkZcoffwJALjLX9RAK4UEYz+YDEaGO7dhSLd1QJbBT8H2ekhxEItTwCSp70JmDx8xltjGfgdzhGQjAWY8iEkp2wrOg8WFu6g==");
// var publicKeyStr = XC.RSAUtil.RsaKeyConvert.PublicKeyPemToXml("MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC/q3kMS8MWnPhFugPSYa/9UiBBQ1KP+cpSO6NtvBPRiBpic9iRNPAcM2sl8wAUIplX2uWHUq3ENQkWkAQaEs5MJ8hZA168OS7Qn+exkLsxR5EjREHGbeIzH1iG5ekHn0ymdc9/bwXba2bzOIaHTNfZT1SuyUdBixYMWshZlVqzuwIDAQAB");
//
// Console.WriteLine($"privateKeyStr:{privateKeyStr}");
// Console.WriteLine($"publicKeyStr:{publicKeyStr}");
//
// var ci = "EoXxXstYtcpCUTr8BxKBqZlQD+hHg7q8diBCElz/iCRe4u8jZD8vN9T0lapknNHPEG7WUXZpgavN2qU4RgKxb1xfjWxe3cdRXzGH4O9cJmIQxOy6PZX5F6Tv0qgr9dKZ7UncCIwh127S+6UgFpGcbt71KOwE5Scc1wbo8VFjlBI=";
// var sa = "hK0b1fIiuFX8jxE3FNitAANZ0K0lxDUjnhQucsVpi6cyF13SdpvdhNvTHrS05WS/YFTh5JP4cAwAUnKyz5MEVAvizhb51UGe44x0ieJHfBJ6z+NWLLdDR8gvoUuED7/gLn5ejDdjFqZUuW07EUcZGmj5hvcQGMxliWAoMY3DmF8=";
// var sign = "";
// var data = "RUWevIIJ+HC4uLq5HfDb7yRMuaXw2RIvVfyYgy92hyxxJAReVEQynlBT0CDF4AfIz3F6F5LbwP0Uke5LNdoUvTmRhjcv4Rcd3YL6D0wxzmmTu7VLXka9hu6n9notC7Sgk2yARle5ZvIxR78lMU+o6u7cV/YqeW1dkts9/f51gw0=";
//
// var aesKey = EncryptUtility.RsaDecrypt(ci, privateKeyStr);
// Console.WriteLine(aesKey);
//
// var aesIv = EncryptUtility.RsaDecrypt(sa, privateKeyStr);
// Console.WriteLine(aesIv);
//
// var toEncryptArray = Convert.FromBase64String(data);
//
// var des = Aes.Create();
// des.Key = Encoding.UTF8.GetBytes(aesKey);
// des.Mode = CipherMode.CBC;
// des.IV = Encoding.UTF8.GetBytes(aesIv);
//
// var cTransform = des.CreateDecryptor();
// var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
// Console.WriteLine(Encoding.Default.GetString(resultArray));


// 有序Id
// Console.WriteLine(Guid.NewGuid());
// Console.WriteLine(SequentialGuidUtility.Next(SequentialGuidType.AsString));
// var snowflakeId = SnowflakeIdUtility.Default().NextId();
// Console.WriteLine($"Findx:" + snowflakeId);
// var options = new IdGeneratorOptions { WorkerIdBitLength = 10, SeqBitLength = 12 };
// YitIdHelper.SetIdGenerator(options);
// Console.WriteLine($"YitIdHelper:" + YitIdHelper.NextId());
// var newLifeSnowflakeId = new Snowflake();
// Console.WriteLine($"NewLife.Snowflake:" + newLifeSnowflakeId.NewId());
// Console.WriteLine($"Ulid.NewUlid:" + Ulid.NewUlid());
//
// Console.WriteLine();
// Console.WriteLine("有序Id生成预热结束");
// Console.WriteLine();
//
// var cyclesCount = 1_000_000;
// var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
//
// var watch = Stopwatch.StartNew();
// for (var i = 0; i < cyclesCount; i++)
// {
//     Guid.NewGuid();
// }
// watch.Stop();
// Console.WriteLine($"原生NewGuid执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     SequentialGuidUtility.Next(SequentialGuidType.AsString);
// }
// watch.Stop();
// Console.WriteLine($"Abp有序Guid执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     NewId.NextSequentialGuid();
// }
// watch.Stop();
// Console.WriteLine($"NewId有序Guid执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     SnowflakeIdUtility.Default().NextId();
// }
// watch.Stop();
// Console.WriteLine($"Findx.Snowflake执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     newLifeSnowflakeId.NewId();
// }
// watch.Stop();
// Console.WriteLine($"NewLife.Snowflake执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     YitIdHelper.NextId();
// }
// watch.Stop();
// Console.WriteLine($"YitIdHelper.NextId()执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
// watch.Restart();
// for (var i = 0; i < cyclesCount; i++)
// {
//     Ulid.NewUlid();
// }
// watch.Stop();
// Console.WriteLine($"Ulid.NewUlid()执行{cyclesCount}次耗时:{watch.Elapsed.TotalMilliseconds}ms");
//
//
//
// // 重复验证
// Console.WriteLine();
// var repeatList = new long[cyclesCount];;
// Parallel.For(0, cyclesCount, i =>
// {
//     repeatList[i] = SnowflakeIdUtility.Default().NextId();
// });
// Console.WriteLine($"并行{cyclesCount}执行,有序Id-SnowflakeIdUtility是否有重复:{repeatList.Distinct().Count() != cyclesCount}");
//
// var repeatList2 = new Guid[cyclesCount];;
// Parallel.For(0, cyclesCount, i =>
// {
//     repeatList2[i] = NewId.NextSequentialGuid();
// });
// Console.WriteLine($"并行{cyclesCount}执行,有序Id-NewId是否有重复:{repeatList2.Distinct().Count() != cyclesCount}");
//
// var repeatList3= new long[cyclesCount];;
// Parallel.For(0, cyclesCount, i =>
// {
//     repeatList3[i] = YitIdHelper.NextId();
// });
// Console.WriteLine($"并行{cyclesCount}执行,有序Id-YitIdHelper是否有重复:{repeatList3.Distinct().Count() != cyclesCount}");
//
// // 连续性
// Console.WriteLine();
// Console.WriteLine($"生成{cyclesCount}个Id检查是否是连续Guid......如有重复将进行行打印...");
// var sequentialList = new List<long>();
// for (var i = 0; i < cyclesCount; i++)
// {
//     sequentialList.Add(SnowflakeIdUtility.Default().NextId());
// }
// var newGuids = sequentialList.OrderBy(x => x).ToList();
// for (var i = 0; i < cyclesCount; i++)
// {
//     if (newGuids[i] != sequentialList[i])
//         Console.WriteLine($"发现非连续:{newGuids[i]} != {sequentialList[i]}");
// }
// Console.WriteLine($"有序Id连续检查结果打印结束.");
//
// Console.WriteLine();
// Console.WriteLine("雪花id 1829316827960315904 反解析...");
// SnowflakeIdUtility.TryParse(1829316827960315904, out var timestamp, out var wid, out var sequence);
// Console.WriteLine(timestamp.ToString(CultureInfo.InvariantCulture));
// Console.WriteLine(wid);
// Console.WriteLine(sequence);


// Json表达式解析
// var entities = new List<SysAppInfo>();
// for (var i = 0; i < 1000; i++)
// {
//     entities.Add(new SysAppInfo
//     {
//         Id = NewId.NextSequentialGuid(),
//         Name = "Name" + (i + 1),
//         Code = "Code" + (i + 1),
//         // Status = (i % 2) > 0 ? 0 : 1,
//         Status = (i % 2) > 0 ? CommonStatus.Success : CommonStatus.Failed,
//         Sort = i
//     });
// }


// // 排序表达式
// var dataSort = SortConditionBuilder.New<SysAppInfo>().OrderBy("Status").OrderBy(x => new { x.Code, x.Id }).Build();
//
// // 自定义筛选器
// var filterGroup = new FilterGroup
// {
//     Logic = FilterOperate.And,
//     Filters =
//     [
//         new FilterCondition
//         {
//             Field = "name", Value = "Name110", Operator = FilterOperate.Contains
//         },
//
//         // new FilterCondition
//         // {
//         //     Field = "Status", Value = "0,2", Operator = FilterOperate.In
//         // }
//     ]
// };
// var st = DateTime.Now;
// var filter = LambdaExpressionParser.ParseConditions<SysAppInfo>(filterGroup);
// Console.WriteLine($"LambdaExpressionParser:{(DateTime.Now - st).TotalMilliseconds}ms");
//
// var s = entities.Where(filter.Compile()).OrderBy("Status", ListSortDirection.Ascending).ThenBy("Sort", ListSortDirection.Descending);
// Console.WriteLine($"{entities.Count}---{s.Count()}");
// foreach (var item in s)
// {
//     Console.WriteLine(item.ToJson());
// }
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

// Console.WriteLine(GenerateWorkerIdBaseOnMac());
// static long GenerateWorkerIdBaseOnMac()
// {
//     var nice = NetworkInterface.GetAllNetworkInterfaces();
//     // exclude virtual and Loopback
//     var firstUpInterface = nice.OrderByDescending(x => x.Speed).FirstOrDefault(x => !x.Description.Contains("Virtual") && x.NetworkInterfaceType != NetworkInterfaceType.Loopback && x.OperationalStatus == OperationalStatus.Up);
//     if (firstUpInterface == null) throw new Exception("no available mac found");
//     var address = firstUpInterface.GetPhysicalAddress();
//     var mac = address.GetAddressBytes();
//
//     return ((mac[4] & 0B11) << 8) | (mac[5] & 0xFF);
// }


// Console.WriteLine(EncryptUtility.Md5By32("123456"));
// Console.WriteLine(EncryptUtility.Sha256("123456"));
//
// EncryptUtility.ToHexString("kjsdhfgkljsgljyhaueghbfsdjghulr昆明的风俗");
// var begin = DateTime.Now;
// for (int i = 0; i < 100000; i++)
// {
//     EncryptUtility.ToHexString("kjsdhfgkljsgljyhaueghbfsdjghulr昆明的风俗");
// }
// Console.WriteLine($"{(DateTime.Now - begin).TotalMilliseconds}");
//
// var one = await CompressionUtility.CompressByBrotliAsync("123456789".ToBytes());
// Console.WriteLine(one.Length);
// var two = await CompressionUtility.DecompressByBrotliAsync(one);
// Console.WriteLine(two.Length);
// Console.WriteLine(Encoding.Default.GetString(two));



// Expression 性能比较

// var t = new SysAppInfo { Id = NewId.NextSequentialGuid(), Code = "test", Name = "史册" };
// var entityType = t.GetType();
// var propertyName = "Name";
// var repeatTimes = 1_000_000;
//
// var expressionGetter = PropertyUtility.ExpressionGetter<SysAppInfo>(propertyName);
// var emitGetter = PropertyUtility.EmitGetter<SysAppInfo>(propertyName);
// PropertyValueGetter<SysAppInfo>.GetPropertyValueObject(entityType, t, propertyName);
// var propertyDynamicGetter = new PropertyDynamicGetter<SysAppInfo>();
//
// var val1 = expressionGetter(t);
// var val2 = emitGetter(t);
// var val3 = propertyDynamicGetter.GetPropertyValue(t, propertyName);
// Console.WriteLine($"t.Name Get:{val1}-{val2}-{val3}");
//     
// var stopwatch = new Stopwatch();  
// stopwatch.Start();
// for (var i = 0; i < repeatTimes; i++)
// {
//     _ = t.Name;
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, 原生属性值读取耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     _ = expressionGetter(t);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, ExpressionGetter实例属性值读取耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     emitGetter(t);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, EmitGetter实例属性值读取耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     PropertyValueGetter<SysAppInfo>.GetPropertyValueObject(entityType, t, propertyName);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, PropertyValueGetter字典缓存实例属性值读取耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     propertyDynamicGetter.GetPropertyValue(t, propertyName);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, PropertyDynamicGetter静态变量缓存实例属性值读取耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// Expression[] CreateParameterExpressions(ParameterInfo[] parameters, Expression arguments)
// {
//     var expressions = new Expression[parameters.Length];
//     for (var i = 0; i < parameters.Length; i++)
//     {
//         var parameter = parameters[i];
//         var argument = Expression.ArrayIndex(arguments, Expression.Constant(i));
//         expressions[i] = Expression.Convert(argument, parameter.ParameterType);
//     }
//
//     return expressions;
// }

// var op = new Op();
// var op2 = new Op();
// var op3 = new Op();
// var op4 = new Op();
// var methodInfo = op.GetType().GetMethod("Say");
// var fastInvoke = FastInvokeHandler.Create(methodInfo);
// var instance = Expression.Parameter(typeof(object), "instance");
// var arguments = Expression.Parameter(typeof(object[]), "arguments");
// var methodCall = Expression.Call(Expression.Convert(instance, op.GetType()), methodInfo!, CreateParameterExpressions(methodInfo?.GetParameters(), arguments));
// var lambdaExpression = Expression.Lambda<Action<object, object[]>>(methodCall, instance, arguments);;
// var expressionCall = lambdaExpression.Compile();
//
// Console.WriteLine($"Op.Say方法执行");
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     op.Say(i);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, 结果:{op}, 方法原生执行:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     fastInvoke.Invoke(op2, [i]);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, 结果:{op2}, FastInvokeHandler方法执行耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     expressionCall(op3, [i]);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, 结果:{op3}, Expression.Call方法执行耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// stopwatch.Reset();
// stopwatch.Restart();
// for (var i = 0; i < repeatTimes; i++)
// {
//     methodInfo.Invoke(op4, parameters: [i]);
// }
// stopwatch.Stop();
// Console.WriteLine($"Repeated {repeatTimes}, 结果:{op4}, MethodInfo.Invoke方法执行耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// internal class Op
// {
//     private int i;
//
//     public void Say(int index)
//     {
//         i += index;
//     }
//
//     public override string ToString()
//     {
//         return i.ToString();
//     }
// }


// var uri = new Uri("ws://106.54.160.19:10020/ws");
//
// var hub = new HubConnection("ws://106.54.160.19:10020/ws", false);
// var stopwatch = new Stopwatch();  
// stopwatch.Start();
// await hub.StartAsync();
// stopwatch.Stop();
// Console.WriteLine($"远程连接执行耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
//
// hub.On((a, b) => { Console.WriteLine(a); return Task.CompletedTask; });
//
// stopwatch.Restart();
// await hub.SendAsync(new ArraySegment<byte>("dotnet --info".ToBytes()), WebSocketMessageType.Text, true);
// stopwatch.Stop();
// Console.WriteLine($"发送消息耗时:{stopwatch.Elapsed.TotalMilliseconds}ms");
// Console.ReadLine();

// var ai = new SysAppInfo { OrgId = Guid.Parse("3a067a31-b7b2-3c48-b770-a987a73c93c3") };
// // Console.WriteLine(ai.OrgId.CastTo<string>());
//
// var bt = JsonSerializer.SerializeToUtf8Bytes(ai);
// Console.WriteLine(bt);
// var str = Encoding.Default.GetString(bt);
// Console.WriteLine(str);

