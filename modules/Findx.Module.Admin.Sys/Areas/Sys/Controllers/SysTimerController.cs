using System;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Module.Admin.Sys.DTO;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Findx.Security.Authorization;
using Findx.Jobs;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统定时任务
    /// </summary>
    [Area("api/sys")]
	[Route("[area]/sysTimers")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysTimerController: ControllerBase
	{
		private readonly IJobStorage _storage;
		private readonly IJobFinder _finder;
        private readonly IJobScheduler _scheduler;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="finder"></param>
        /// <param name="scheduler"></param>
        public SysTimerController(IJobStorage storage, IJobFinder finder, IJobScheduler scheduler)
        {
            _storage = storage;
            _finder = finder;
            _scheduler = scheduler;
        }

        /// <summary>
        /// 查询定时任务列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("page")]
		public async Task<CommonResult> PageAsync([FromQuery] SysTimerQuery req)
        {
			var list = await _storage.GetJobsAsync();
			var skip = req.PageNo > 1 ? req.PageNo * req.PageSize : 0;
            list = list.WhereIf(!req.taskName.IsNullOrWhiteSpace(), x => x.Name.Contains(req.taskName))
                       .WhereIf(req.jobStatus.HasValue && req.jobStatus.Value == 0, x => x.IsEnable == false)
                       .WhereIf(req.jobStatus.HasValue && req.jobStatus.Value == 1, x => x.IsEnable == true)
                       .Skip(skip).Take(req.PageSize);
            var res = new PageResult<IEnumerable<JobInfo>>(req.PageNo, req.PageSize, list.Count(), list);
			return CommonResult.Success(res);
		}

        /// <summary>
        /// 查询可执行定时任务类
        /// </summary>
        /// <returns></returns>
		[HttpPost("getActionClasses")]
		public CommonResult GetActionClasses()
        {
            var typeList = _finder.FindAll(true);
            var res = new List<string>();

            foreach(var item in typeList)
            {
                res.Add(item.FullName);
            }
            return CommonResult.Success(res);
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("stop")]
        public async Task<CommonResult> Stop([FromBody] SysTimerRequest req)
        {
            var model = await _storage.FindAsync(req.Id.To<long>());
            if (model == null)
                return CommonResult.Fail("job.404", "定时任务不存在");
            if (!model.IsEnable)
                return CommonResult.Fail("job.401", "定时任务已经是停止状态");

            model.IsEnable = false;
            await _storage.UpdateAsync(model);

            return CommonResult.Success();
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("start")]
        public async Task<CommonResult> Start([FromBody] SysTimerRequest req)
        {
            var model = await _storage.FindAsync(req.Id.To<long>());
            if (model == null || model.IsEnable)
                return CommonResult.Fail("job.404", "定时任务不存在");
            if (model.IsEnable)
                return CommonResult.Fail("job.401", "定时任务已经是启用状态");

            model.IsEnable = true;
            await _storage.UpdateAsync(model);

            return CommonResult.Success();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<CommonResult> Delete([FromBody] SysTimerRequest req)
        {
            await _storage.DeleteAsync(req.Id.To<long>());
            return CommonResult.Success();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<CommonResult> Add([FromBody] SysTimerRequest req)
        {
            var typeInfo = _finder.FindAll(true).FirstOrDefault(x => x.FullName == req.FullName);
            if (typeInfo == null)
                return CommonResult.Fail("job.class.404", "定时任务class类不存在");

            var taskInfo = new JobInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                CronExpress = req.CronExpress,
                IsEnable = true,
                IsSingle = false,
                NextRunTime = Utils.Cron.GetNextOccurrence(req.CronExpress),
                Id = Findx.Utils.SnowflakeId.Default().NextId(),
                JsonParam = req.JsonParam,
                Name = req.Name,
                FullName = req.FullName,
                TryCount = 0,
            };

            await _storage.InsertAsync(taskInfo);

            return CommonResult.Success();
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public async Task<CommonResult> Edit([FromBody] SysTimerRequest req)
        {
            var typeInfo = _finder.FindAll(true).FirstOrDefault(x => x.FullName == req.FullName);
            if (typeInfo == null)
                return CommonResult.Fail("job.class.404", "定时任务class类不存在");

            var model = await _storage.FindAsync(req.Id.To<long>());
            if (model == null)
                return CommonResult.Fail("job.404", "定时任务不存在");

            model.CronExpress = req.CronExpress;
            model.NextRunTime = Utils.Cron.GetNextOccurrence(req.CronExpress);
            model.JsonParam = req.JsonParam;
            model.Name = req.Name;
            model.FullName = req.FullName;

            await _storage.UpdateAsync(model);

            return CommonResult.Success();
        }
    }
}

