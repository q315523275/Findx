﻿using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Findx.Mapping;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Enum;
using Findx.Module.EleAdmin.Models;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    /// <summary>
    /// 授权服务
    /// </summary>
    [Area("system")]
    [Route("api/[area]/auth")]
    public class AuthController : AreaApiControllerBase
    {
        private readonly IJwtTokenBuilder _tokenBuilder;
        private readonly IOptions<JwtOptions> _options;
        private readonly ICurrentUser _currentUser;
        private readonly ICacheProvider _cacheProvider;
        private readonly IRepository<SysUserInfo> _repo;
        private readonly IRepository<SysLoginRecordInfo> _recordRepo;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="tokenBuilder"></param>
        /// <param name="options"></param>
        /// <param name="currentUser"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="repo"></param>
        public AuthController(IJwtTokenBuilder tokenBuilder, IOptions<JwtOptions> options, ICurrentUser currentUser, ICacheProvider cacheProvider, IRepository<SysUserInfo> repo, IRepository<SysLoginRecordInfo> recordRepo)
        {
            _tokenBuilder = tokenBuilder;
            _options = options;
            _currentUser = currentUser;
            _cacheProvider = cacheProvider;
            _repo = repo;
            _recordRepo = recordRepo;
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("/api/login")]
        public async Task<CommonResult> Login([FromBody] LoginRequest req)
        {
            // 验证码正确性验证
            var cache = _cacheProvider.Get();
            if (cache.Get<string>($"verifyCode:" + req.Uuid) != req.Code.ToLower())
                return CommonResult.Fail("50500", "验证码错误");

            var accountInfo = await _repo.FirstAsync(it => it.UserName == req.UserName && it.TenantId == req.TenantId);
            // 验证帐号是否存在
            if (accountInfo == null)
                return CommonResult.Fail("D1000", "账户不存在");

            // 系统参数
            var userAgent = HttpContext.Request.GetUserAgent();
            var loginLog = new SysLoginRecordInfo
            {
                Browser = userAgent.Browser.Name,
                Comments = "账户密码错误",
                CreatedTime = DateTime.Now,
                Device = userAgent.OS.Name,
                Id = Findx.Utils.SequentialGuid.Instance.Create(DatabaseType.MySql),
                Ip = HttpContext.GetClientIp(),
                LoginType = 1,
                Os = userAgent.Platform.Name,
                TenantId = req.TenantId,
                UserName = req.UserName,
                Nickname = accountInfo.Nickname
            };
            CommonResult fail = null;
            // 验证帐号密码是否正确
            if (accountInfo.Password != Findx.Utils.Encrypt.Md5By32(req.Password))
            {
                loginLog.LoginType = 1;
                loginLog.Comments = "账户密码错误";
                fail = CommonResult.Fail("D1000", "账户密码错误");
            }

            // 验证账号是否被冻结
            if (accountInfo.Status != CommonStatusEnum.ENABLE.To<int>())
            {
                loginLog.LoginType = 1;
                loginLog.Comments = "账号已冻结";
                return CommonResult.Fail("D1017", "账号已冻结");
            }

            // 清空验证码
            cache.Remove("verifyCode:" + req.Uuid);

            // 登录日志
            if (fail != null)
            {
                _recordRepo.Insert(loginLog);
                return fail;
            }
            loginLog.LoginType = 0;
            loginLog.Comments = "登录成功";
            _recordRepo.Insert(loginLog);

            // token票据
            var payload = new Dictionary<string, string>
            {
                { ClaimTypes.UserId, accountInfo.Id.SafeString() },
                { ClaimTypes.PhoneNumber, accountInfo.Phone.SafeString() },
                { ClaimTypes.Name, accountInfo.Nickname.SafeString() },
                { ClaimTypes.TenantId, req.TenantId.ToString() }
            };
            var token = await _tokenBuilder.CreateAsync(payload, _options.Value);

            return CommonResult.Success(new
            {
                access_token = "Bearer " + token.AccessToken
            });
        }

        /// <summary>
        /// 用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/auth/user")]
        [Authorize]
        public new CommonResult User()
        {
            var userId = _currentUser.UserId.To<Guid>();
            var tenantId = _currentUser.TenantId.To<Guid>();
            var userInfo = _repo.First(x => x.Id == userId);
            if (userInfo == null)
                return CommonResult.Fail("D1000", "账户不存在");

            var roleRepo = HttpContext.RequestServices.GetRequiredService<IRepository<SysUserRoleInfo>>();
            var menuRepo = HttpContext.RequestServices.GetRequiredService<IRepository<SysRoleMenuInfo>>();

            var roles = roleRepo.Select(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id, selectExpression: x => new RoleDto { Id = x.RoleId, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name }).DistinctBy(x => x.Id);
            var roleIds = roles.Select(x => x.Id);
            var menus = roleIds.Count() > 0 ?
                               menuRepo.Select(x => roleIds.Contains(x.RoleId) && x.MenuId == x.MenuInfo.Id,
                                   selectExpression: x => new MenuDto { MenuId = x.MenuId })
                               : new List<MenuDto>();

            var result = userInfo.MapTo<UserAuthDto>();
            result.Roles = roles;
            result.Authorities = menus.DistinctBy(x => x.MenuId).OrderBy(x => x.Sort);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("/api/auth/password")]
        [Authorize]
        public CommonResult Password([FromBody] UpdatePasswordRequest req)
        {
            var userId = _currentUser.UserId.To<Guid>();
            var tenantId = _currentUser.TenantId.To<Guid>();
            var userInfo = _repo.First(x => x.Id == userId);
            if (userInfo == null)
                return CommonResult.Fail("D1000", "账户不存在");

            if (userInfo.Password != Findx.Utils.Encrypt.Md5By32(req.OldPassword))
                return CommonResult.Fail("D1000", "旧密码错误");

            var pwd = Findx.Utils.Encrypt.Md5By32(req.Password);
            _repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == userInfo.Id);

            return CommonResult.Success();
        }
    }
}

