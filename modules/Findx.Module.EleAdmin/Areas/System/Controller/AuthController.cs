using Findx.AspNetCore.Mvc;
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

        private readonly IRepository<SysUserInfo> _repo;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="tokenBuilder"></param>
        /// <param name="options"></param>
        /// <param name="currentUser"></param>
        /// <param name="repo"></param>
        public AuthController(IJwtTokenBuilder tokenBuilder, IOptions<JwtOptions> options, ICurrentUser currentUser, IRepository<SysUserInfo> repo)
        {
            _tokenBuilder = tokenBuilder;
            _options = options;
            _currentUser = currentUser;
            _repo = repo;
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("/api/login")]
        public async Task<CommonResult> Login([FromBody] LoginRequest req)
        {
            var accountInfo = _repo.First(it => it.UserName == req.UserName && it.TenantId == req.TenantId);
            // 验证帐号是否存在
            if (accountInfo == null)
                return CommonResult.Fail("D1000", "账户不存在");

            // 验证帐号密码是否正确
            if (accountInfo.Password != Findx.Utils.Encrypt.Md5By32(req.Password))
            {
                return CommonResult.Fail("D1000", "账户密码错误");
            }

            // 验证账号是否被冻结
            if (accountInfo.Status != CommonStatusEnum.ENABLE.To<int>())
            {
                return CommonResult.Fail("D1017", "账号已冻结");
            }

            // token票据
            var payload = new Dictionary<string, string>
            {
                { ClaimTypes.UserId, accountInfo.Id.SafeString() },
                { ClaimTypes.PhoneNumber, accountInfo.Phone.SafeString() },
                { ClaimTypes.Name, accountInfo.RealName.SafeString() },
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

            var roles = roleRepo.Select(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id).DistinctBy(x => x.RoleId);
            var roleIds = roles.Select(x => x.RoleId);
            var menus = menuRepo.Select(x => roleIds.Contains(x.RoleId) && x.MenuId == x.MenuInfo.Id);

            var result = userInfo.MapTo<UserAuthDto>();
            result.Roles = roles.Select(x => new RoleDto { Code = x.RoleInfo?.Code, Name = x.RoleInfo?.Name });
            result.Authorities = menus.Select(x => x.MenuInfo).OrderBy(x => x.Sort).MapTo<List<MenuDto>>();

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
            {
                return CommonResult.Fail("D1000", "旧密码错误");
            }

            var pwd = Findx.Utils.Encrypt.Md5By32(req.Password);
            _repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == userInfo.Id);

            return CommonResult.Success();
        }
    }
}

