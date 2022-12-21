// See https://aka.ms/new-console-template for more information

using System.Net.NetworkInformation;
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

Console.WriteLine($"localIp:{DnsUtil.ResolveHostAddress(DnsUtil.ResolveHostName())}");

var monitorNetwork = new MonitorNetwork("en6");
monitorNetwork.Start();
while (true)
{
    Thread.Sleep(1000);
    Console.WriteLine($"全部流量:{monitorNetwork.AllTraffic};上行速率:{monitorNetwork.UpSpeed};下行速率:{monitorNetwork.DownSpeed}");
}

//IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
//NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
//Console.WriteLine("Interface information for {0}.{1}     ",
//        computerProperties.HostName, computerProperties.DomainName);
//if (nics == null || nics.Length < 1)
//{
//    Console.WriteLine("  No network interfaces found.");
//    return;
//}
//foreach (NetworkInterface adapter in nics)
//{
//    System.Threading.Tasks.Task.Factory.StartNew(() =>
//    {
//        while (true)
//        {
//            var start = adapter.GetIPv4Statistics().BytesReceived;
//            System.Threading.Thread.Sleep(1000);
//            var end = adapter.GetIPv4Statistics().BytesReceived;
//            var res = (end - start);
//            if (res > 0)
//            {
//                Console.WriteLine($"{adapter.Name}:{(res / 1024)} kb，speed:{adapter.Speed / (1024 * 1024)}MB");
//            }
//        }
//    });
//}

//while (true)
//{
//    Thread.Sleep(5000);
//}


