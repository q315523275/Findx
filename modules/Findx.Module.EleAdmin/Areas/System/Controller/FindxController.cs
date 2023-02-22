﻿using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc;
using Findx.Builders;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    /// <summary>
    /// Findx框架
    /// </summary>
    [Area("system")]
    [Route("api/[area]/findx")]
    // [Authorize]
    public class FindxController: AreaApiControllerBase
    {
        /// <summary>
        /// 模块列表
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="keyGenerator"></param>
        /// <returns></returns>
        [HttpGet("modules")]
        [Description("模块列表")]
        public CommonResult Modules([FromServices] IFindxBuilder builder, [FromServices] IKeyGenerator<long> keyGenerator)
        {
            var res = builder.Modules.Select(m => new
            {
                m.GetType().Name,
                Display = m.GetType().GetDescription(true),
                Class = m.GetType().FullName,
                m.Level,
                m.Order,
                m.IsEnabled,
                Id = keyGenerator.Create()
            });
            return CommonResult.Success(res);
        }

        /// <summary>
        /// 系统指标
        /// </summary>
        /// <returns></returns>
        [HttpGet("metrics")]
        [Description("系统指标")]
        public async Task<CommonResult> Metrics([FromServices] IApplicationContext app)
        {
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            var runtimeInfo = new Dictionary<string, object>
            {
                { "cpu", (await Findx.Utils.RuntimeUtil.GetCpuUsage()).ToString("0.000") },
                { "memory", (Findx.Utils.RuntimeUtil.GetMemoryUsage() / 1024).ToString("0.000") + "/" + (GC.GetTotalMemory(false) / 1024.0 / 1024.0).ToString("0.000") },
                { "pid", Findx.Utils.RuntimeUtil.GetPid().ToString() },
                { "processorCount", Findx.Utils.RuntimeUtil.GetProcessorCount().ToString() },
                { "runTime", Findx.Utils.RuntimeUtil.GetRunTime().TotalMinutes.ToString("0.00") },
                { "virtualMemory", (Findx.Utils.RuntimeUtil.GetVirtualMemory() / 1024.0).ToString(CultureInfo.InvariantCulture) },
                { "threadCount", Findx.Utils.RuntimeUtil.GetThreadCount().ToString() },
                { "handleCount", Findx.Utils.RuntimeUtil.GetHandleCount().ToString() },
                { "applicationName", app.ApplicationName },
                { "applicationPort", app.Port.ToString() },
                { "uris", string.Join(",", app.Uris) },
                { "rootPath", app.RootPath },
                { "instanceIp", app.InstanceIp },
                { "threadStats", new {
                    maxCompletionPortThreads = maxCompletionPortThreads,
                    maxWorkerThreads = maxWorkerThreads,
                    availableCompletionPortThreads = availableCompletionPortThreads,
                    availableWorkerThreads = availableWorkerThreads
                }}
            };
           
            var sysOsInfo = new Dictionary<string, string>
            {
                { "osArchitecture", RuntimeInformation.OSArchitecture.ToString() },
                { "osDescription", RuntimeInformation.OSDescription },
                { "processArchitecture", RuntimeInformation.ProcessArchitecture.ToString() },
                { "frameworkDescription", RuntimeInformation.FrameworkDescription },
                { "is64BitOperatingSystem", Environment.Is64BitOperatingSystem.ToString() },
                { "instanceIP", app.InstanceIp },
                { "machineName", Environment.MachineName },
                { "osVersion", Environment.OSVersion.ToString() },
                { "systemPageSize", Environment.SystemPageSize.ToString() },
                { "version", Environment.Version.ToString() },
                { "userDomainName", Environment.UserDomainName },
                { "userInteractive", Environment.UserInteractive.ToString() },
                { "userName", Environment.UserName.ToString() },
                { "drives", string.Join(";", Environment.GetLogicalDrives()) },
                { "osName", Findx.Utils.Common.System },
                { "tickCount", $"{Environment.TickCount / 1000 / 60}" }
            };

            var sysDict = Findx.Utils.SystemUtil.GetMachineInfo();
            foreach(var item in sysDict)
            {
                sysOsInfo.TryAdd(item.Key, item.Value.SafeString());
            }
            
            return CommonResult.Success(new { sysOsInfo, runtimeInfo });
        }
        
        /// <summary>
        /// 方法集合
        /// </summary>
        /// <returns></returns>
        [HttpGet("functions")]
        public CommonResult Functions([FromServices] IFunctionStore<MvcFunction> store)
        {
            return CommonResult.Success(store.GetFromDatabase());
        }
        
        
        [HttpGet("test")]
        [AllowAnonymous]
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
}

