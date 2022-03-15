using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 通知公告
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysNotice")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysNoticeController : CrudControllerBase<SysNoticeInfo, SysNoticeOutput, SysNoticeCreateRequest, SysNoticeUpdateRequest, SysNoticeQuery, long, long>
    {
        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysNoticeInfo> CreatePageWhereExpression(SysNoticeQuery request)
        {
            return ExpressionBuilder.Create<SysNoticeInfo>().AndIF(request.Type > 0, x => x.Type == request.Type)
                                                            .AndIF(!request.SearchValue.IsNullOrWhiteSpace(), x => x.Title.Contains(request.SearchValue));
        }

        private SysNoticeCreateRequest _currentCreateRequest;
        private SysNoticeUpdateRequest _currentUpdateRequest;

        protected override SysNoticeInfo ToModelFromCreateRequest(SysNoticeCreateRequest request)
        {
            _currentCreateRequest = request;

            return base.ToModelFromCreateRequest(request);
        }

        protected override SysNoticeInfo ToModelFromUpdateRequest(SysNoticeUpdateRequest request)
        {
            _currentUpdateRequest = request;
            return base.ToModelFromUpdateRequest(request);
        }

        protected override async Task AddBeforeAsync(SysNoticeInfo model)
        {
            var currentUser = GetService<ICurrentUser>();
            if (!currentUser.IsAuthenticated) return;

            var repo_emp = GetRepository<SysEmpInfo>();

            var empInfo = await repo_emp.GetAsync(currentUser.UserId);

            model.PublicUserId = currentUser.UserId.To<long>();
            model.PublicUserName = currentUser.UserName;
            model.PublicOrgId = empInfo.OrgId;
            model.PublicOrgName = empInfo.OrgName;

            if (model.Status == NoticeStatusEnum.PUBLIC.To<int>())
                model.PublicTime = DateTime.Now;
        }

        protected override async Task AddAfterAsync(SysNoticeInfo model, int result)
        {
            if (result > 0)
            {
                var noticeUserIdList = _currentCreateRequest.NoticeUserIdList;
                var noticeUserList = noticeUserIdList.Select(u => new SysNoticeUserInfo
                {
                    NoticeId = model.Id,
                    UserId = u,
                    Status = NoticeUserStatusEnum.UNREAD.To<int>(),
                    Id = Findx.Utils.SnowflakeId.Default().NextId()
                }).ToList();
                var repo = GetRepository<SysNoticeUserInfo>();
                await repo.InsertAsync(noticeUserList);
            }
        }

        protected override async Task EditBeforeAsync(SysNoticeInfo model)
        {
            var currentUser = GetService<ICurrentUser>();
            if (!currentUser.IsAuthenticated) return;
            var repo_emp = GetRepository<SysEmpInfo>();

            var empInfo = await repo_emp.GetAsync(currentUser.UserId);

            model.PublicUserId = currentUser.UserId.To<long>();
            model.PublicUserName = currentUser.UserName;
            model.PublicOrgId = empInfo.OrgId;
            model.PublicOrgName = empInfo.OrgName;

            if (model.Status == NoticeStatusEnum.PUBLIC.To<int>())
                model.PublicTime = DateTime.Now;
        }

        protected override async Task EditAfterAsync(SysNoticeInfo model, int result)
        {
            if (result > 0)
            {
                var noticeUserIdList = _currentUpdateRequest.NoticeUserIdList;
                var noticeUserList = noticeUserIdList.Select(u => new SysNoticeUserInfo
                {
                    NoticeId = model.Id,
                    UserId = u,
                    Status = NoticeUserStatusEnum.UNREAD.To<int>(),
                    Id = Findx.Utils.SnowflakeId.Default().NextId()
                }).ToList();
                var repo = GetRepository<SysNoticeUserInfo>();
                await repo.DeleteAsync(x => x.NoticeId == model.Id);
                await repo.InsertAsync(noticeUserList);
            }
        }

        protected override async Task DetailAfterAsync(SysNoticeInfo model)
        {
            var currentUser = GetService<ICurrentUser>();
            var userId = currentUser?.UserId?.CastTo<long>();
            var repo = GetRepository<SysNoticeUserInfo>();
            // 如果该条通知公告为已发布，则将当前用户的该条通知公告设置为已读
            if (model.Status == NoticeStatusEnum.PUBLIC.To<int>())
                await repo.UpdateColumnsAsync(x => new SysNoticeUserInfo { ReadTime = DateTime.Now, Status = 1 }, x => x.NoticeId == model.Id && x.UserId == userId);
        }

        protected override SysNoticeOutput ToDto(SysNoticeInfo model)
        {
            var currentUser = GetService<ICurrentUser>();
            var userName = currentUser?.UserName;

            var noticeResult = base.ToDto(model);

            var repo = GetRepository<SysNoticeUserInfo>();
            // 获取通知到的用户
            var noticeUserList = repo.Select(x => x.NoticeId == model.Id);
            var noticeUserIdList = new List<string>();
            var noticeUserReadInfoList = new List<NoticeUserRead>();
            if (noticeUserList != null)
            {
                noticeUserList.ForEach(u =>
                {
                    noticeUserIdList.Add(u.UserId.ToString());
                    var noticeUserRead = new NoticeUserRead
                    {
                        UserId = u.UserId,
                        UserName = userName,
                        ReadStatus = u.Status,
                        ReadTime = u.ReadTime
                    };
                    noticeUserReadInfoList.Add(noticeUserRead);
                });
            }
            noticeResult.NoticeUserIdList = noticeUserIdList;
            noticeResult.NoticeUserReadInfoList = noticeUserReadInfoList;

            return noticeResult;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpPost("changeStatus")]
        public CommonResult ChangeStatus([FromBody] SysUserStatusUpdateRequest request, [FromServices] IRepository<SysNoticeInfo> repo)
        {
            // 用户信息
            var info = repo.Get(request.Id);
            if (info == null)
                return CommonResult.Fail("404", "数据不存在");

            if (!System.Enum.IsDefined(typeof(NoticeStatusEnum), request.Status))
                return CommonResult.Fail("404", "字典状态错误");

            var updateColums = new List<Expression<Func<SysNoticeInfo, bool>>>
            {
                x => x.Status == request.Status
            };
            if (request.Status == NoticeStatusEnum.PUBLIC.To<int>())
                updateColums.Add(x => x.PublicTime == DateTime.Now);
            if (request.Status == NoticeStatusEnum.CANCEL.To<int>())
                updateColums.Add(x => x.CancelTime == DateTime.Now);

            updateColums.Add(x => x.UpdateTime == DateTime.Now);

            repo.UpdateColumns(updateColums, x => x.Id == request.Id);

            return CommonResult.Success();
        }

        /// <summary>
        /// 获取接收的通知公告
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("received")]
        public async Task<CommonResult> ReceivedNoticePageList([FromQuery] SysNoticeQuery request)
        {
            var fsql = GetService<IFreeSql>();
            var currentUser = GetService<ICurrentUser>();
            var userId = currentUser?.UserId?.CastTo<long>();


            var searchValue = !string.IsNullOrEmpty(request.SearchValue?.Trim());

            var list = await fsql.Select<SysNoticeInfo, SysNoticeUserInfo>()
                                 .InnerJoin((n, u) => n.Id == u.NoticeId)
                                 .Where((n, u) => u.UserId == userId)
                                 .WhereIf(searchValue, (n, u) => n.Title.Contains(request.SearchValue))
                                 .WhereIf(request.Type > 0, (n, u) => n.Type == request.Type)
                                 .Count(out var totalRows)
                                 .Page(request.PageNo, request.PageSize)
                                 .ToListAsync((n, u) => new { n.Title, n.Type, n.Content, u.Status, ReadTime = u.ReadTime });

            return CommonResult.Success(new PageResult<object>(request.PageNo, request.PageSize, (int)totalRows, list));
        }
    }
}
