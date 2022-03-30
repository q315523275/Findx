using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Module.Admin.Const;
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Findx.Security.Authorization;
using Findx.Module.Admin.Captcha;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 账户服务
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/account")]
    [ApiExplorerSettings(GroupName = "system")]
    public class AccountController : AreaApiControllerBase
    {
        private readonly IClickWordCaptcha _captchaHandle; // 验证码服务
        private readonly JwtOptions _options;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="_optionsMonitor"></param>
        public AccountController(IOptionsMonitor<JwtOptions> _optionsMonitor)
        {
            _options = _optionsMonitor.CurrentValue;
        }

        private void SaveVisLog(SysUserInfo userInfo, LoginTypeEnum loginType, string result, string msg)
        {
            var userAgent = HttpContext.Request.GetUserAgent();
            // 登录成功，记录登录日志
            SysVisLogInfo visLog = new SysVisLogInfo
            {
                Account = userInfo.Account,
                Browser = userAgent?.Browser.Name,
                Ip = HttpContext.GetClientIp(),
                Os = userAgent?.OS?.Name,
                Message = msg,
                VisTime = DateTime.Now,
                Name = loginType.ToDescription(),
                VisType = loginType.To<int>(),
                Success = result
            };
            var repo = HttpContext.RequestServices.GetService<IRepository<SysVisLogInfo>>();
            visLog.SetEmptyKey();
            repo.Insert(visLog);
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userRepo"></param>
        /// <param name="tokenBuilder"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<CommonResult> Login([FromBody] LoginRequest request, [FromServices] IRepository<SysUserInfo> userRepo, [FromServices] IRepository<SysEmpInfo> empRepo, [FromServices] IJwtTokenBuilder tokenBuilder)
        {
            var accountInfo = userRepo.First(it => it.Account == request.Account);
            // 验证帐号是否存在
            if (accountInfo == null)
                return CommonResult.Fail("D1000", "账户不存在");
            // 验证帐号密码是否正确
            if (accountInfo.Password != Findx.Utils.Encrypt.Md5By32(request.Password))
            {
                SaveVisLog(accountInfo, LoginTypeEnum.LOGIN, "N", "账户密码错误");
                return CommonResult.Fail("D1000", "账户密码错误");
            }
            // 验证账号是否被冻结
            if (accountInfo.Status != CommonStatusEnum.ENABLE.To<int>())
            {
                SaveVisLog(accountInfo, LoginTypeEnum.LOGIN, "N", "账号已冻结");
                return CommonResult.Fail("D1017", "账号已冻结");
            }

            // 员工信息
            var empInfo = empRepo.Get(accountInfo.Id);

            // 更新登陆信息
            accountInfo.LastLoginIp = HttpContext.GetClientIp();
            accountInfo.LastLoginTime = DateTime.Now;
            userRepo.UpdateColumns(x => new SysUserInfo { LastLoginIp = accountInfo.LastLoginIp, LastLoginTime = accountInfo.LastLoginTime }, x => x.Id == accountInfo.Id);

            SaveVisLog(accountInfo, LoginTypeEnum.LOGIN, "Y", null);

            // token票据
            var payload = new Dictionary<string, string>
            {
                { ClaimTypes.UserId, accountInfo.Id.SafeString() },
                { ClaimTypes.PhoneNumber, accountInfo.Phone.SafeString() },
                { ClaimTypes.FullName, accountInfo.Name.SafeString() },
                { ClaimTypes.Role, "admin" },
                { ClaimConst.SUPER_ADMIN, accountInfo.AdminType.SafeString() },
                { ClaimConst.ORG_ID, empInfo.OrgId.SafeString() },
                { ClaimConst.ORG_NAME, empInfo.OrgName.SafeString() }
            };
            var token = await tokenBuilder.CreateAsync(payload, _options);

            return CommonResult.Success<string>(token.AccessToken);
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        [Authorize(Roles = "admin")]
        public CommonResult LoginOut([FromServices] IPrincipal principal)
        {
            var userId = principal.Identity.GetUserId<long>();
            var repo = HttpContext.RequestServices.GetService<IRepository<SysUserInfo>>();
            // 用户信息
            var userInfo = repo.First(x => x.Id == userId);
            if (userInfo == null)
                return CommonResult.Success();

            SaveVisLog(userInfo, LoginTypeEnum.LOGOUT, "Y", null);

            return CommonResult.Success();
        }

        /// <summary>
        /// 查询登录用户信息
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        [HttpGet("getLoginUser")]
        [Authorize(Roles = "admin")]
        public CommonResult GetLoginUser([FromServices] IFreeSql fsql, [FromServices] IPrincipal principal, [FromServices] IMapper mapper)
        {
            var userId = principal.Identity.GetUserId<long>();

            // 用户信息
            var userInfo = fsql.Select<SysUserInfo>(userId).First();
            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");

            var superAdmin = userInfo.IsSuperAdmin();
            var loginUserDTO = mapper.MapTo<LoginUserDTO>(userInfo);
            loginUserDTO.Enabled = userInfo.Status == CommonStatusEnum.ENABLE.To<int>();

            var userAgent = HttpContext.Request.GetUserAgent();

            loginUserDTO.LastLoginBrowser = userAgent?.Browser.Name;
            loginUserDTO.LastLoginOs = userAgent?.OS.Name;
            // loginUserDTO.LastLoginIp = HttpContext.GetClientIp();
            // loginUserDTO.LastLoginTime = DateTime.Now;

            // 员工信息
            var loginEmpDTO = new LoginEmpDTO();
            var empInfo = fsql.Select<SysEmpInfo>(userId).First();
            if (empInfo != null)
            {
                loginEmpDTO.JobNum = empInfo.JobNum;
                loginEmpDTO.OrgId = empInfo.OrgId;
                loginEmpDTO.OrgName = empInfo.OrgName;
                loginEmpDTO.ExtOrgPos = fsql.Select<SysEmpExtOrgPosInfo, SysOrgInfo, SysPosInfo>()
                                            .LeftJoin((a, b, c) => a.OrgId == b.Id)
                                            .LeftJoin((a, b, c) => a.PosId == c.Id)
                                            .Where((a, b, c) => a.EmpId == empInfo.Id && b.Status == 0 && c.Status == 0)
                                            .ToList((a, b, c) => new { OrgCode = b.Code, OrgName = b.Name, PosCode = c.Code, PosName = c.Name });
                loginEmpDTO.Positions = fsql.Select<SysEmpPosInfo, SysPosInfo>().LeftJoin((a, b) => a.PosId == b.Id).Where((a, b) => a.EmpId == empInfo.Id && b.Status == 0)
                                            .ToList((a, b) => new { PosCode = b.Code, PosName = b.Name });
            }
            else
            {
                loginEmpDTO.ExtOrgPos = new List<string>();
                loginEmpDTO.Positions = new List<string>();
            }
            loginUserDTO.LoginEmpInfo = loginEmpDTO;

            // 角色信息
            var roles = fsql.Select<SysRoleInfo, SysUserRoleInfo>().InnerJoin((a, b) => a.Id == b.RoleId && b.UserId == userId).Where((a, b) => a.Status == 0)
                            .ToList((a, b) => new { a.Id, a.Name, a.Code, a.DataScopeType });
            var roleIdList = roles.Select(it => it.Id);
            loginUserDTO.Roles = roles;

            // 权限信息
            var permissions = new List<string>();
            if (roleIdList.Count() > 0)
            {
                var permissionList = fsql.Select<SysMenuInfo, SysRoleMenuInfo>().InnerJoin((a, b) => a.Id == b.MenuId)
                                         .Where((a, b) => roleIdList.Contains(b.RoleId) && a.Status == 0 && a.Type == 2)
                                         .ToList((a, b) => new { a.Type, a.Permission, a.Router });
                foreach (var item in permissionList)
                {
                    if (MenuTypeEnum.BTN.To<int>() == item.Type)
                    {
                        permissions.Add(item.Permission);
                    }
                    else
                    {
                        string removePrefix = item.Router.RemovePreFix(SymbolConst.LEFT_DIVIDE);
                        string permission = removePrefix.Replace(SymbolConst.LEFT_DIVIDE, SymbolConst.COLON);
                        permissions.Add(permission);
                    }
                }
            }
            loginUserDTO.Permissions = permissions;

            // 数据范围信息
            var dataScopes = fsql.Select<SysUserDataScopeInfo>().Where(it => it.UserId == userId).ToList(it => it.OrgId); // 用户直接分配范围
            if (empInfo != null && roleIdList.Count() > 0)
            {
                // 自定义的数据范围角色ID集合
                var customDataScopeRoleIdList = new List<long>();
                var strongerDataScopeType = DataScopeTypeEnum.SELF.To<int>();
                foreach (var sysRole in roles)
                {
                    if (DataScopeTypeEnum.DEFINE.To<int>() == sysRole.DataScopeType)
                    {
                        // 自定义的数据范围
                        customDataScopeRoleIdList.Add(sysRole.Id);
                    }
                    else
                    {
                        // 限定自身机构数据范围
                        if (sysRole.DataScopeType <= strongerDataScopeType)
                        {
                            strongerDataScopeType = sysRole.DataScopeType;
                        }
                    }
                }
                // 自定义数据范围的角色对应的数据范围
                var dataScopes2 = fsql.Select<SysRoleDataScopeInfo>().Where(it => customDataScopeRoleIdList.Contains(it.RoleId)).ToList(it => it.OrgId);
                // 角色中拥有最大数据范围类型的数据范围
                var dataScopes3 = new List<long>();
                // 如果是范围类型是全部数据，则获取当前系统所有的组织架构id
                if (DataScopeTypeEnum.ALL.To<int>() == strongerDataScopeType)
                {
                    dataScopes3 = fsql.Select<SysOrgInfo>().Where(it => it.Status == 0).ToList(it => it.Id);
                }
                // 如果范围类型是本部门及以下部门，则查询本节点和子节点集合，包含本节点
                else if (DataScopeTypeEnum.DEPT_WITH_CHILD.To<int>() == strongerDataScopeType)
                {
                    var likeValue = $"{SymbolConst.LEFT_SQUARE_BRACKETS}{empInfo.OrgId}{SymbolConst.RIGHT_SQUARE_BRACKETS}";
                    dataScopes3 = fsql.Select<SysOrgInfo>().Where(it => it.Pids.Contains(likeValue) && it.Status == 0).ToList(it => it.Id);
                }
                // 如果数据范围是本部门，不含子节点，则直接返回本部门
                else if (DataScopeTypeEnum.DEPT.To<int>() == strongerDataScopeType)
                {
                    dataScopes3.Add(empInfo.OrgId);
                }
                dataScopes.AddRange(dataScopes2);
                dataScopes.AddRange(dataScopes3);
            }
            loginUserDTO.DataScopes = dataScopes;

            // 具备应用信息（多系统，默认激活一个，可根据系统切换菜单）,返回的结果中第一个为激活的系统
            var firstAppCode = string.Empty;
            if (superAdmin)
            {
                var appList = fsql.Select<SysAppInfo>().Where(it => it.Status == 0).OrderByDescending(x => x.Active).OrderBy(x => x.Sort).ToList(it => new { it.Code, it.Name, Active = it.Active == "Y" });
                loginUserDTO.Apps = appList;
                firstAppCode = appList.FirstOrDefault(it => it.Active)?.Code ?? appList.FirstOrDefault()?.Code;
            }
            else if (roleIdList.Count() > 0)
            {
                // 获取用户菜单对应的应用编码集合
                var appCodeList = fsql.Select<SysMenuInfo, SysRoleMenuInfo>().InnerJoin((a, b) => a.Id == b.MenuId)
                                      .Where((a, b) => roleIdList.Contains(b.RoleId) && a.Status == 0).Distinct().ToList((a, b) => a.Application);
                // 当应用编码不为空时，则限制查询范围
                var appList = fsql.Select<SysAppInfo>().Where(it => appCodeList.Contains(it.Code) && it.Status == 0).OrderByDescending(x => x.Active).OrderBy(x => x.Sort).ToList(it => new { it.Code, it.Name, Active = it.Active == "Y" });
                loginUserDTO.Apps = appList;
                firstAppCode = appList.FirstOrDefault(it => it.Active)?.Code ?? appList.FirstOrDefault()?.Code;
            }

            // 如果根本没有应用信息，则没有菜单信息
            if (firstAppCode.IsNullOrWhiteSpace())
            {
                loginUserDTO.Menus = new List<string>();
            }
            else
            {
                var menuList = fsql.Select<SysMenuInfo, SysRoleMenuInfo>().LeftJoin((a, b) => a.Id == b.MenuId)
                                   .WhereIf(!superAdmin, (a, b) => roleIdList.Contains(b.RoleId))
                                   //.WhereIf(superAdmin, (a, b) => a.Weight != 2) // 超级管理不碰业务
                                   .Where((a, b) => a.Application == firstAppCode && a.Type != 2 && a.Status == 0)
                                   .OrderBy((a, b) => a.Sort)
                                   .ToList((a, b) => new LoginUserMenuDTO
                                   {
                                       OpenType = a.OpenType,
                                       Id = a.Id,
                                       Pid = a.Pid,
                                       Name = a.Code,
                                       Component = a.Component,
                                       Redirect = a.Redirect,
                                       Path = a.Router,
                                       Hidden = a.Visible == "N",
                                       Meta = new LoginUserMenuMetaDTO
                                       {
                                           Title = a.Name,
                                           Icon = a.Icon,
                                           Show = a.Visible == "Y",
                                           Link = a.Link
                                       }
                                   }).DistinctBy2(x => x.Id);
                foreach (var item in menuList)
                {
                    if (MenuOpenTypeEnum.OUTER.To<int>() == item.OpenType)
                    {
                        item.Meta.Target = "_blank";
                        item.Path = item.Meta.Link;
                        item.Redirect = item.Meta.Link;
                    }
                }
                loginUserDTO.Menus = menuList;
            }

            return CommonResult.Success(loginUserDTO);
        }
    }
}
