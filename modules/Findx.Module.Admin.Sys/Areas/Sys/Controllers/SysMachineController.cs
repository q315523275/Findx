using Findx.AspNetCore.Mvc;
using Findx.Linq;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Findx.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Threading;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 机器信息
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysMachine")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysMachineController : AreaApiControllerBase
    {

        [HttpGet("query")]
        public async Task<CommonResult> QueryAsync([FromServices] IApplicationContext app)
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
                { "virtualMemory", (Findx.Utils.RuntimeUtil.GetVirtualMemory() / 1024.0).ToString() },
                { "threadCount", Findx.Utils.RuntimeUtil.GetThreadCount().ToString() },
                { "handleCount", Findx.Utils.RuntimeUtil.GetHandleCount().ToString() },
                { "applicationName", app.ApplicationName },
                { "applicationPort", app.Port.ToString() },
                { "uris", string.Join(",", app.Uris) },
                { "rootPath", app.RootPath },
                { "instanceIP", app.InstanceIP },
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
                { "instanceIP", app.InstanceIP },
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
    }
}
