﻿// See https://aka.ms/new-console-template for more information

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Findx.Extensions;
using Findx.Machine;
using Findx.Machine.Cpu;
using Findx.Machine.Memory;
using Findx.Machine.Network;
using Findx.Metrics;
using Findx.Utils;

Console.WriteLine("Hello, World!");

// var a = Findx.Utils.RegexUtil.GetValue("abc4d5e6hh5654", @"\d+");
// Console.WriteLine($"a的值为：{a};");
// var b = Findx.Utils.RegexUtil.GetValues("abc4d5e6hh5654", @"\d+");
// Console.WriteLine($"b的值为：{JsonSerializer.Serialize(b)}");
//
// // receive string result from stdout.
// var version = await ProcessX.StartAsync("dotnet --version").FirstAsync();
// Console.WriteLine(version);

var network = NetworkInfo.GetNetworkInfos().FirstOrDefault(x => x.Name.Contains("eth0")); //TryGetRealNetworkInfo();
if (network == null) return;
var oldRate = network.IpvSpeed();
var oldRateLength = oldRate.ReceivedLength + oldRate.SendLength;
var networkSpeed = SizeInfo.Get(network.Speed);
var v1 = CpuHelper.GetCpuTime();

while (true)
{
    Thread.Sleep(1000);
    
    var v2 = CpuHelper.GetCpuTime();
    var value = CpuHelper.CalculateCpuLoad(v1, v2);
    v1 = v2;

    var memory = MemoryHelper.GetMemoryValue();
    var newRate = network.IpvSpeed();
    var nodeRate = SizeInfo.Get(newRate.ReceivedLength + newRate.SendLength - oldRateLength);
    var speed = NetworkInfo.GetSpeed(oldRate, newRate);
    oldRate = newRate;
    
    Console.Clear();
    Console.WriteLine($"Cpu:{(int)(value * 100)} %");
    Console.WriteLine($"已用内存:{memory.UsedPercentage} %");
    Console.WriteLine($"网卡信息:{network.Name},{network.UnicastAddresses.FirstOrDefault()};Ips:{NetworkInfo.GetIpAddresses().Select(x => x.MapToIPv4().ToString()).ToJson()}");
    Console.WriteLine($"网卡连接速度:{networkSpeed.Size} {networkSpeed.SizeType}/s");
    Console.WriteLine($"监测流量:{nodeRate.Size} {nodeRate.SizeType} 上传速率:{speed.Sent.Size} {speed.Sent.SizeType}/s 下载速率:{speed.Received.Size} {speed.Received.SizeType}/s");
}
