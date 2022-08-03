﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc;
using Findx.Builders;
using Findx.Data;
using Findx.Email;
using Findx.Extensions;
using Findx.Module.EleAdmin.Jobs;
using Findx.Security;

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
        public CommonResult Modules([FromServices] IFindxBuilder builder,
            [FromServices] IKeyGenerator<long> keyGenerator)
        {
            var res = builder.Modules.Select(m => new
            {
                m.GetType().Name,
                Display = m.GetType().GetDescription(true),
                Class = m.GetType().FullName,
                m.Level,
                m.Order,
                m.IsEnabled,
                Id = Findx.Utils.SnowflakeId.Default().NextId()
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

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <returns></returns>
        [HttpGet("addJobs")]
        public async Task<CommonResult> AddJobs([FromServices] Findx.Jobs.IJobScheduler scheduler)
        {
            for (var i = 0; i < 10000; i++)
            {
                await scheduler.ScheduleAsync<TestJobTask>(TimeSpan.FromSeconds(5));
            }
            return CommonResult.Success();
        }
        
        /// <summary>
        /// 方法集合
        /// </summary>
        /// <returns></returns>
        [HttpGet("functions")]
        public CommonResult AddJobs([FromServices] IFunctionStore<MvcFunction> store)
        {
            return CommonResult.Success(store.GetFromDatabase());
        }
        
        /// <summary>
        /// 邮件发送示例接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpGet("emailSend")]
        public async Task<string> EmailSend([FromServices] IEmailSender sender, [Required] string mailAddress, [Required] string subject, [Required] string body)
        {
            await sender.SendAsync(mailAddress, subject, body, isBodyHtml: false);

            return "ok";
        }
    }
}
