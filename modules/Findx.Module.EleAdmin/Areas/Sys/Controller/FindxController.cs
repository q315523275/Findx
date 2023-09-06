using System.ComponentModel;
using System.Diagnostics;
using Findx.AspNetCore.Mvc;
using Findx.Builders;
using Findx.Data;
using Findx.Extensions;
using Findx.Machine;
using Findx.Machine.Cpu;
using Findx.Machine.Memory;
using Findx.Machine.Network;
using Findx.Machine.System;
using Findx.Security;
using Findx.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Findx.Module.EleAdmin.Areas.Sys.Controller;

/// <summary>
///     Findx框架
/// </summary>
[Area("system")]
[Route("api/[area]/findx")]
[Description("Findx框架")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("Findx框架")]
public class FindxController : AreaApiControllerBase
{
    /// <summary>
    ///     模块列表
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("modules")]
    [Description("模块列表")]
    [DisableAuditing]
    public CommonResult Modules([FromServices] IFindxBuilder builder, [FromServices] IKeyGenerator<Guid> keyGenerator)
    {
        var res = builder.Modules.Select(m => new
        {
            m.GetType().Name,
            Display = m.GetType().GetDescription(),
            Class = m.GetType().FullName,
            m.Level,
            m.Order,
            m.IsEnabled,
            Id = keyGenerator.Create()
        });
        return CommonResult.Success(res);
    }

    /// <summary>
    ///     系统指标
    /// </summary>
    /// <returns></returns>
    [HttpGet("metrics")]
    [Description("系统指标")]
    [DisableAuditing]
    public async Task<CommonResult> Metrics([FromServices] IApplicationContext app)
    {
        var dict = new Dictionary<string, object>
        {
            {
                "System", new
                {
                    SystemPlatformInfo.MachineName,
                    SystemPlatformInfo.SystemDirectory,
                    SystemPlatformInfo.FrameworkDescription,
                    SystemPlatformInfo.FrameworkVersion,
                    SystemPlatformInfo.UserName,
                    SystemPlatformInfo.UserDomainName,
                    SystemPlatformInfo.OsArchitecture,
                    SystemPlatformInfo.OsDescription,
                    SystemPlatformInfo.OsVersion,
                    SystemPlatformInfo.OsPlatformId,
                    SystemPlatformInfo.ProcessorCount,
                    SystemPlatformInfo.ProcessArchitecture,
                    SystemPlatformInfo.GetLogicalDrives,
                    SystemPlatformInfo.IsUserInteractive
                }
            }
        };
        var network = NetworkInfo.TryGetRealNetworkInfo();
        var oldRate = network.IpvSpeed();
        var oldRateLength = oldRate.ReceivedLength + oldRate.SendLength;
        var networkSpeed = SizeInfo.Get(network.Speed);
        var v1 = CpuHelper.GetCpuTime();
        
        await Task.Delay(1000);
        
        var cpuValue = CpuHelper.CalculateCpuLoad(v1, CpuHelper.GetCpuTime());
        dict.Add("Cpu", $"{(int)(cpuValue * 100)} %");
        
        var memory = MemoryHelper.GetMemoryValue();
        dict.Add("Memory", new
        {
            AvailablePhysicalMemory = SizeInfo.Get((long)memory.AvailablePhysicalMemory).ToString(),
            AvailableVirtualMemory = SizeInfo.Get((long)memory.AvailableVirtualMemory).ToString(),
            TotalPhysicalMemory = SizeInfo.Get((long)memory.TotalPhysicalMemory).ToString(),
            TotalVirtualMemory = SizeInfo.Get((long)memory.TotalVirtualMemory).ToString(),
            memory.UsedPercentage,
            UsedPhysicalMemory = SizeInfo.Get((long)memory.UsedPhysicalMemory).ToString(),
            UsedVirtualMemory = SizeInfo.Get((long)memory.UsedVirtualMemory).ToString(),
        });
        
        var newRate = network.IpvSpeed();
        var nodeRate = SizeInfo.Get(newRate.ReceivedLength + newRate.SendLength - oldRateLength);
        var speed = NetworkInfo.GetSpeed(oldRate, newRate);
        
        dict.Add("NetworkInfo", new
        {
            网卡信息 = new { network.Name, network.Mac, RealIpv4 = NetworkInfo.TryGetRealIpv4().ToString() },
            网卡连接速度 = $"{networkSpeed.Size} {networkSpeed.SizeType}/s",
            监测流量 = $"{nodeRate.Size} {nodeRate.SizeType}",
            总流量 = $"{SizeInfo.Get(oldRateLength).Size} {SizeInfo.Get(oldRateLength).SizeType}",
            上传速率 = $"{speed.Sent.Size} {speed.Sent.SizeType}/s",
            下载速率 = $"{speed.Received.Size} {speed.Received.SizeType}/s"
        });

        dict.Add("RuntimeInfo", new
        {
            Ip = app.InstanceIp,
            Cpu = (await RuntimeUtil.GetCpuUsage()).ToString("0.000"),
            Memory = SizeInfo.Get(Process.GetCurrentProcess().WorkingSet64).ToString(),
            ThreadCount = RuntimeUtil.GetThreadCount(),
            HandleCount = RuntimeUtil.GetHandleCount()
        });
        
        var gcInfo = RuntimeUtil.GetGcInfo();
        dict.Add("GcInfo", new
        {
            gen0 = SizeInfo.Get(gcInfo.gen0).ToString(),
            gen1 = SizeInfo.Get(gcInfo.gen1).ToString(),
            gen2 = SizeInfo.Get(gcInfo.gen2).ToString(),
            totalMemory = SizeInfo.Get(gcInfo.totalMemory).ToString(),
        });

        return CommonResult.Success(dict);
    }

    /// <summary>
    ///     方法集合
    /// </summary>
    /// <returns></returns>
    [HttpGet("functions")]
    [DisableAuditing]
    public CommonResult Functions([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(store.GetFromDatabase());
    }

    /// <summary>
    ///     test
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("test")]
    [DisableAuditing]
    public CommonResult Test([FromServices] ILogger<FindxController> logger)
    {
        try
        {
            throw new Exception("这是一条来自测试的异常");
        }
        catch (Exception e)
        {
            logger.LogError(e, "findx");
        }

        return CommonResult.Success();
    }
}