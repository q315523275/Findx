using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Findx.Module.Admin.Sys.Filters;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysUser")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysUserController : CrudControllerBase<SysUserInfo, SysUserOutput, SysUserCreateRequest, SysUserUpdateRequest, SysUserQuery, long, long>
    {
        private readonly IFreeSql fsql;
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        public SysUserController(IFreeSql fsql, ICurrentUser currentUser)
        {
            this.fsql = fsql;
            _currentUser = currentUser;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DataScope]
        [HttpGet("page")]
        public override async Task<CommonResult> PageAsync([FromQuery] SysUserQuery request)
        {
            Check.NotNull(request, nameof(request));

            var userId = _currentUser.UserId.To<long>();
            var dataScope = new List<long>();
            if (!_currentUser.IsSuperAdmin())
            {
                dataScope = _currentUser.DataScope();
            }

            var pid = request.SysEmpParam != null && !request.SysEmpParam.OrgId.IsNullOrWhiteSpace() ? request.SysEmpParam.OrgId.To<long>() : 0;
            var key = $"[{request.SysEmpParam?.OrgId}]";

            var userList = await fsql.Select<SysUserInfo, SysEmpInfo, SysOrgInfo>()
                                     .InnerJoin((u, e, o) => u.Id == e.Id)
                                     .InnerJoin((u, e, o) => e.OrgId == o.Id)
                                     .WhereIf(!request.SearchValue.IsNullOrWhiteSpace(), (u, e, o) => u.Account.Contains(request.SearchValue) || u.Name.Contains(request.SearchValue) || u.Phone.Contains(request.SearchValue))
                                     .WhereIf(pid > 0, (u, e, o) => e.OrgId == pid || o.Pids.Contains(key))
                                     .WhereIf(request.SearchStatus > -1, (u, e, o) => u.Status == request.SearchStatus)
                                     .WhereIf(!_currentUser.IsSuperAdmin(), (u, e, o) => u.AdminType != 1 && dataScope.Contains(e.OrgId))
                                     .Count(out var totalRows)
                                     .Page(request.PageNo, request.PageSize)
                                     .ToListAsync<SysUserOutput>();

            return CommonResult.Success(new PageResult<List<SysUserOutput>>(request.PageNo, request.PageSize, (int)totalRows, userList));
        }

        /// <summary>
        /// 详情结果转换
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override SysUserOutput ToDetailDTO(SysUserInfo result)
        {
            var userOutput = result.MapTo<SysUserOutput>();
            Check.NotNull(userOutput, nameof(userOutput));

            var sysEmpInfo = fsql.Select<SysEmpInfo>().Where(it => it.Id == userOutput.Id).First<SysEmpOutput>();
            if (sysEmpInfo == null)
                return userOutput;

            userOutput.SysEmpInfo = sysEmpInfo;
            userOutput.SysEmpInfo.ExtOrgPos = fsql.Select<SysEmpExtOrgPosInfo>().Where(it => it.EmpId == userOutput.Id).ToList<SysEmpExtOrgPosOutput>();
            userOutput.SysEmpInfo.Positions = fsql.Select<SysEmpPosInfo>().Where(it => it.EmpId == userOutput.Id).ToList<SysEmpPosOutput>();

            return userOutput;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DataScope]
        [HttpPost("add")]
        public override async Task<CommonResult> AddAsync([FromBody] SysUserCreateRequest request)
        {
            var repo = GetRepository<SysUserInfo>();
            var repo_emp = GetRepository<SysEmpInfo>();
            var repo_emp_ext = GetRepository<SysEmpExtOrgPosInfo>();
            var repo_emp_pos = GetRepository<SysEmpPosInfo>();
            var currentUser = GetService<ICurrentUser>();

            // 如果登录用户不是超级管理员，则进行数据权限校验
            if (!_currentUser.IsSuperAdmin())
            {
                List<long> dataScope = _currentUser.DataScope();
                // 获取添加的用户的所属机构
                long orgId = request.SysEmpParam.OrgId.To<long>();
                // 数据范围为空
                if (dataScope.IsNullOrEmpty())
                {
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
                else if (!dataScope.Contains(orgId))
                {
                    // 所添加的用户的所属机构不在自己的数据范围内
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
            }

            var isExist = await repo.ExistAsync(u => u.Account == request.Account);
            if (isExist)
                return CommonResult.Fail("D1003", "账号已存在");

            var user = request.MapTo<SysUserInfo>();
            Check.NotNull(user, nameof(user));

            user.Password = Findx.Utils.Encrypt.Md5By32(request.Password);
            if (string.IsNullOrEmpty(user.Name))
                user.Name = user.Account;
            if (string.IsNullOrEmpty(user.NickName))
                user.NickName = user.Account;
            user.AdminType = AdminTypeEnum.None.To<int>();
            user.CreatedTime = DateTime.Now;
            user.CreatorId = (long?)(currentUser?.UserId?.CastTo(user.CreatorId.GetType()));
            user.SetEmptyKey();

            // 事物
            var uow = repo.GetUnitOfWork();
            uow.BeginOrUseTransaction();
            try
            {
                await AddBeforeAsync(user);
                var res = repo.Insert(user);
                await AddAfterAsync(user, res);
                if (res < 1)
                    return CommonResult.Fail("db.add.error", "数据创建失败");

                // 员工信息
                request.SysEmpParam.Id = user.Id;

                // 先删除员工信息
                await repo_emp.DeleteAsync(x => x.Id == request.SysEmpParam.Id);
                var emp = request.SysEmpParam.MapTo<SysEmpInfo>();
                await repo_emp.InsertAsync(emp);

                // 更新附属机构职位信息
                // 先删除
                await repo_emp_ext.DeleteAsync(x => x.EmpId == request.SysEmpParam.Id);
                var extOrgPos = request.SysEmpParam.ExtIds.Select(u => new SysEmpExtOrgPosInfo { EmpId = request.SysEmpParam.Id, OrgId = u.OrgId, PosId = u.PosId, Id = Utils.SnowflakeId.Default().NextId() }).ToList();
                await repo_emp_ext.InsertAsync(extOrgPos);

                // 更新职位信息
                // 先删除
                await repo_emp_pos.DeleteAsync(x => x.EmpId == request.SysEmpParam.Id);
                var empPos = request.SysEmpParam.PosIdList.Select(u => new SysEmpPosInfo { EmpId = request.SysEmpParam.Id, PosId = u, Id = Utils.SnowflakeId.Default().NextId() }).ToList();
                await repo_emp_pos.InsertAsync(empPos);

                uow.Commit();

                return CommonResult.Success();
            }
            catch (Exception ex)
            {
                uow.Rollback();

                Logger.LogError(ex, null);

                return CommonResult.Fail("db.add.error", ex.Message);
            }
        }

        /// <summary>
        /// 编辑修改
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DataScope]
        [HttpPost("edit")]
        public override async Task<CommonResult> EditAsync([FromBody] SysUserUpdateRequest request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<SysUserInfo>();
            var repo_emp = GetRepository<SysEmpInfo>();
            var repo_emp_ext = GetRepository<SysEmpExtOrgPosInfo>();
            var repo_emp_pos = GetRepository<SysEmpPosInfo>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            // 如果登录用户不是超级管理员，则进行数据权限校验
            if (!_currentUser.IsSuperAdmin())
            {
                List<long> dataScope = _currentUser.DataScope();
                // 获取添加的用户的所属机构
                long orgId = request.SysEmpParam.OrgId.To<long>();
                // 数据范围为空
                if (dataScope.IsNullOrEmpty())
                {
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
                else if (!dataScope.Contains(orgId))
                {
                    // 所添加的用户的所属机构不在自己的数据范围内
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
            }

            // 排除自己并且判断与其他是否相同
            var isExist = await repo.ExistAsync(u => u.Account == request.Account && u.Id != request.Id);
            if (isExist)
                return CommonResult.Fail("D1003", "账号已存在");

            var user = request.MapTo<SysUserInfo>();
            Check.NotNull(user, nameof(user));
            user.LastUpdatedTime = DateTime.Now;
            user.LastUpdaterId = (long?)(currentUser?.UserId?.CastTo(user.LastUpdaterId.GetType()));

            // 事物
            var uow = repo.GetUnitOfWork();
            uow.BeginOrUseTransaction();
            try
            {
                await EditBeforeAsync(user);
                var res = await repo.UpdateAsync(user, ignoreColumns: x => new { x.Password, x.Status, x.AdminType, x.CreatedTime, x.CreatorId, x.LastLoginIp, x.LastLoginTime }, ignoreNullColumns: true);
                await EditAfterAsync(user, res);

                // 员工信息
                request.SysEmpParam.Id = user.Id;

                // 先删除员工信息
                await repo_emp.DeleteAsync(x => x.Id == request.SysEmpParam.Id);
                var emp = request.SysEmpParam.MapTo<SysEmpInfo>();
                await repo_emp.InsertAsync(emp);

                // 更新附属机构职位信息
                // 先删除
                await repo_emp_ext.DeleteAsync(x => x.EmpId == request.SysEmpParam.Id);
                var extOrgPos = request.SysEmpParam.ExtIds.Select(u => new SysEmpExtOrgPosInfo { Id = Utils.SnowflakeId.Default().NextId(), EmpId = request.SysEmpParam.Id, OrgId = u.OrgId, PosId = u.PosId }).ToList();
                await repo_emp_ext.InsertAsync(extOrgPos);

                // 更新职位信息
                // 先删除
                await repo_emp_pos.DeleteAsync(x => x.EmpId == request.SysEmpParam.Id);
                var empPos = request.SysEmpParam.PosIdList.Select(u => new SysEmpPosInfo { Id = Utils.SnowflakeId.Default().NextId(), EmpId = request.SysEmpParam.Id, PosId = u }).ToList();
                await repo_emp_pos.InsertAsync(empPos);
                uow.Commit();

                return CommonResult.Success();
            }
            catch (Exception ex)
            {
                uow.Rollback();

                Logger.LogError(ex, null);

                return CommonResult.Fail("db.update.error", ex.Message);
            }
        }

        /// <summary>
        /// 删除用户及用户周边信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DataScope]
        [HttpPost("delete")]
        public override async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<long>> request)
        {
            Check.NotNull(request, nameof(request));
            if (request.Count == 0)
                return CommonResult.Fail("delete.not.count", "不存在删除数据");

            var repo = GetRepository<SysUserInfo>();
            var repo_emp = GetRepository<SysEmpInfo>();
            var repo_emp_ext = GetRepository<SysEmpExtOrgPosInfo>();
            var repo_emp_pos = GetRepository<SysEmpPosInfo>();
            var repo_role = GetRepository<SysUserRoleInfo>();
            var repo_data = GetRepository<SysUserDataScopeInfo>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            await DeleteBeforeAsync(request);

            // 操作人数据范围
            List<long> dataScope = _currentUser.DataScope();

            int total = 0;
            foreach (var item in request)
            {
                var user = await repo.FirstAsync(u => u.Id == item.Id);
                if (user == null)
                    return CommonResult.Fail("D1002", "记录不存在");

                if (user.AdminType == AdminTypeEnum.SuperAdmin.To<int>())
                    return CommonResult.Fail("D1014", "禁止删除超级管理员");

                if (user.Id == currentUser.UserId.To<long>())
                    return CommonResult.Fail("D1001", "非法操作，禁止删除自己");

                // 如果登录用户不是超级管理员，则进行数据权限校验
                if (!_currentUser.IsSuperAdmin())
                {
                    var empInfo = await repo_emp.GetAsync(user.Id);
                    // 获取添加的用户的所属机构
                    long orgId = empInfo.OrgId;
                    // 数据范围为空
                    if (dataScope.IsNullOrEmpty())
                    {
                        return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                    }
                    else if (!dataScope.Contains(orgId))
                    {
                        // 所添加的用户的所属机构不在自己的数据范围内
                        return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                    }
                }

                // 直接删除用户
                if (repo.Delete(key: item.Id) > 0)
                    total++;

                // 删除员工及附属机构职位信息
                await repo_emp.DeleteAsync(x => x.Id == item.Id); // empId与userId相同
                await repo_emp_pos.DeleteAsync(x => x.EmpId == item.Id); // empId与userId相同
                await repo_emp_ext.DeleteAsync(x => x.EmpId == item.Id); // empId与userId相同

                //删除该用户对应的用户-角色表关联信息
                await repo_role.DeleteAsync(x => x.UserId == item.Id);

                //删除该用户对应的用户-数据范围表关联信息
                await repo_data.DeleteAsync(x => x.UserId == item.Id);
            }

            await DeleteAfterAsync(request, total);

            return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
        }


        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpPost("changeStatus")]
        public CommonResult ChangeStatus([FromBody] SysUserStatusUpdateRequest request, [FromServices] IRepository<SysUserInfo> repo)
        {
            // 用户信息
            var userInfo = fsql.Select<SysUserInfo>(request.Id).First();
            if (userInfo == null)
                return CommonResult.Fail("404", "用户不存在");

            if (!System.Enum.IsDefined(typeof(CommonStatusEnum), request.Status))
                return CommonResult.Fail("404", "字典状态错误");

            if (userInfo.IsSuperAdmin())
                return CommonResult.Fail("403", "无权限");

            var updateColums = new List<Expression<Func<SysUserInfo, bool>>>
            {
                x => x.Status == request.Status
            };

            repo.UpdateColumns(updateColums, x => x.Id == request.Id);

            return CommonResult.Success();
        }

        /// <summary>
        /// 获取用户选择器
        /// </summary>
        /// <returns></returns>
        [HttpGet("selector")]
        public async Task<CommonResult> Selector([FromQuery] SysUserQuery request)
        {

            var userId = _currentUser.UserId.To<long>();

            var userList = await fsql.Select<SysUserInfo>()
                                     .WhereIf(!request.SearchValue.IsNullOrWhiteSpace(), x => x.Name.Contains(request.SearchValue))
                                     .Where(x => x.AdminType != 1)
                                     .ToListAsync(x => new { x.Id, x.Name });

            return CommonResult.Success(userList);
        }

        /// <summary>
        /// 重置初始化密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("resetPwd")]
        public async Task<CommonResult> ResetPwd([FromBody] SysUserRequest req)
        {
            var repo = GetRepository<SysUserInfo>();

            var password = Findx.Utils.Encrypt.Md5By32("123456");

            await repo.UpdateColumnsAsync(x => new SysUserInfo { Password = password }, x => x.Id == req.Id);

            return CommonResult.Success();
        }

        /// <summary>
        /// 拥有的角色编号集合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("ownRole")]
        public async Task<CommonResult> OwnRole([FromQuery] SysUserRequest req)
        {
            var repo = GetRepository<SysUserRoleInfo>();
            var list = await repo.SelectAsync(x => x.UserId == req.Id, x => x.RoleId);
            return CommonResult.Success(list);
        }

        /// <summary>
        /// 设置用户角色信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [DataScope]
        [HttpPost("grantRole")]
        public async Task<CommonResult> GrantRole([FromBody] SysUserRequest req)
        {
            //如果登录用户不是超级管理员，则进行数据权限校验
            if (!_currentUser.IsSuperAdmin())
            {
                List<long> dataScope = _currentUser.DataScope();
                // 获取要授权角色的用户的所属机构
                var repo_emp = GetRepository<SysEmpInfo>();
                SysEmpInfo sysEmpInfo = repo_emp.Get(req.Id);
                long orgId = sysEmpInfo.OrgId;
                // 数据范围为空
                if (dataScope.IsEmpty())
                {
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
                else if (!dataScope.Contains(orgId))
                {
                    // 所要授权角色的用户的所属机构不在自己的数据范围内
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
            }

            var repo = GetRepository<SysUserRoleInfo>();
            await repo.DeleteAsync(x => x.UserId == req.Id);
            var list = new List<SysUserRoleInfo>();
            req.GrantRoleIdList?.ForEach(x =>
            {
                list.Add(new SysUserRoleInfo { RoleId = x, UserId = req.Id, Id = Findx.Utils.SnowflakeId.Default().NextId() });
            });
            if (list.Count > 0)
                repo.Insert(list);

            return CommonResult.Success();
        }

        /// <summary>
        /// 查询角色拥有的数据ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpGet("ownData")]
        public CommonResult OwnData([FromQuery] SysUserRequest req, [FromServices] IRepository<SysUserDataScopeInfo> repo)
        {
            var ids = repo.Select(x => x.UserId == req.Id, x => x.OrgId);

            return CommonResult.Success(ids);
        }

        /// <summary>
        /// 设置角色数据范围
        /// </summary>
        /// <returns></returns>
        [DataScope]
        [HttpPost("grantData")]
        public CommonResult GrantData([FromBody] SysRoleGrantDataRequest req)
        {
            // 如果登录用户不是超级管理员，则进行数据权限校验
            if (!_currentUser.IsSuperAdmin())
            {
                List<long> dataScope = _currentUser.DataScope();
                // 获取要授权数据的用户的所属机构
                var repo_emp = GetRepository<SysEmpInfo>();
                SysEmpInfo sysEmpInfo = repo_emp.Get(req.Id);
                long orgId = sysEmpInfo.OrgId;
                // 数据范围为空
                if (dataScope.IsEmpty())
                {
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
                else if (!dataScope.Contains(orgId))
                {
                    // 所要授权角色的用户的所属机构不在自己的数据范围内
                    return CommonResult.Fail("D1003", "没有权限操作该数据，请联系管理员");
                }
                // 要授权的数据范围列表
                List<long> grantOrgIdList = req.GrantOrgIdList;
                if (!grantOrgIdList.IsNullOrEmpty())
                {
                    // 数据范围为空
                    if (dataScope.IsNullOrEmpty())
                    {
                        return CommonResult.Fail("D4003", "没有权限操作该数据，请联系管理员");
                    }
                    else if (grantOrgIdList.Except(dataScope).Any()) // 存在差集
                    {
                        // 所要授权的数据不在自己的数据范围内
                        return CommonResult.Fail("D4003", "没有权限操作该数据，请联系管理员");
                    }
                }
            }

            var repo_data = GetRepository<SysUserDataScopeInfo>();

            repo_data.Delete(x => x.UserId == req.Id);

            var list = new List<SysUserDataScopeInfo>();
            req.GrantOrgIdList?.ForEach(x =>
            {
                list.Add(new SysUserDataScopeInfo { OrgId = x, UserId = req.Id, Id = Utils.SnowflakeId.Default().NextId() });
            });
            if (list.Count > 0)
                repo_data.Insert(list);

            return CommonResult.Success();
        }

        /// <summary>
        /// 更新个人信息
        /// </summary>
        [HttpPost("updateInfo")]
        public async Task<CommonResult> UpdateInfo([FromBody] SysUserUpdateInfoRequest req)
        {
            var userId = _currentUser.UserId.To<long>();
            var repo = GetRepository<SysUserInfo>();
            var userInfo = repo.Get(userId);
            if (userInfo == null)
                return CommonResult.Fail("404", "数据不存在");

            await repo.UpdateColumnsAsync(x => new SysUserInfo { NickName = req.NickName, Birthday = req.Birthday, Email = req.Email, Phone = req.Phone, Sex = req.Sex, Tel = req.Tel }
                                        , x => x.Id == userId);

            return CommonResult.Success();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("updatePwd")]
        public async Task<CommonResult> UpdatePwd([FromBody] SysUserUpdatePwdRequest req)
        {
            var userId = _currentUser.UserId.To<long>();
            var repo = GetRepository<SysUserInfo>();
            var userInfo = repo.Get(userId);
            if (userInfo == null)
                return CommonResult.Fail("404", "数据不存在");

            if (Findx.Utils.Encrypt.Md5By32(req.Password) != userInfo.Password)
                return CommonResult.Fail("404", "原始密码错误");

            var newPwd = Findx.Utils.Encrypt.Md5By32(req.Confirm);
            await repo.UpdateColumnsAsync(x => new SysUserInfo { Password = newPwd } , x => x.Id == userId);

            return CommonResult.Success();
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("updateAvatar")]
        public async Task<CommonResult> UpdateAvatar([FromBody] SysUserUpdateAvatarRequest req)
        {
            var userId = _currentUser.UserId.To<long>();
            var repo = GetRepository<SysUserInfo>();
            var userInfo = repo.Get(userId);
            if (userInfo == null)
                return CommonResult.Fail("404", "数据不存在");

            await repo.UpdateColumnsAsync(x => new SysUserInfo { Avatar = req.Avatar.Id }, x => x.Id == userId);

            return CommonResult.Success();
        }

        
    }
}
