using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Findx.Security;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     功能权限授权回归测试
///     <para>覆盖角色解析改造后的语义：Roles 串带空格可正确匹配、去 Intersect 后结果等价、
///     超级角色忽略大小写、空/缺失角色一律 Forbidden（fail-closed）。</para>
/// </summary>
public class FunctionAuthorizationTests
{
    private sealed class TestFunction : FunctionBase
    {
    }

    private sealed class TestAuthorization : FunctionAuthorizationBase
    {
        public TestAuthorization(IConfiguration configuration) : base(configuration)
        {
        }
    }

    private static IConfiguration BuildConfiguration(string superRoleName = null)
    {
        var config = new Dictionary<string, string>();
        if (superRoleName != null) config["Findx:Authorization:SuperRoleName"] = superRoleName;
        return new ConfigurationBuilder().AddInMemoryCollection(config).Build();
    }

    private static IPrincipal PrincipalWithRoles(params string[] roles)
    {
        var claims = roles.Select(role => new Claim(System.Security.Claims.ClaimTypes.Role, role)).ToArray();
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static TestFunction RoleLimitFunction(string roles)
    {
        var function = new TestFunction();
        function.AccessType = FunctionAccessType.RoleLimit;
        function.Roles = roles;
        return function;
    }

    [Fact]
    public void Authorize_RoleLimit_MatchingRole_ReturnsOk()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("admin");

        Assert.Equal(AuthorizationStatus.Ok, authorization.Authorize(function, PrincipalWithRoles("admin")));
    }

    [Fact]
    public void Authorize_RoleLimit_NotMatchingRole_ReturnsForbidden()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("admin");

        Assert.Equal(AuthorizationStatus.Forbidden, authorization.Authorize(function, PrincipalWithRoles("user")));
    }

    [Fact]
    public void Authorize_RoleLimit_RolesWithSpaces_TrimmedAndMatched()
    {
        // 改造后 Split 使用 TrimEntries：数据库里写的 "admin, ops" 之前会解析出 " ops" 导致永不匹配
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("admin, ops");

        Assert.Equal(AuthorizationStatus.Ok, authorization.Authorize(function, PrincipalWithRoles("ops")));
    }

    [Fact]
    public void Authorize_RoleLimit_SuperRole_MatchesIgnoreCase()
    {
        var authorization = new TestAuthorization(BuildConfiguration("super"));
        var function = RoleLimitFunction("admin");

        Assert.Equal(AuthorizationStatus.Ok, authorization.Authorize(function, PrincipalWithRoles("SUPER")));
    }

    [Fact]
    public void Authorize_RoleLimit_NoRoleClaims_ReturnsForbidden()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("admin");

        Assert.Equal(AuthorizationStatus.Forbidden, authorization.Authorize(function, PrincipalWithRoles()));
    }

    [Fact]
    public void Authorize_RoleLimit_NullRoles_ReturnsForbidden()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction(null);

        Assert.Equal(AuthorizationStatus.Forbidden, authorization.Authorize(function, PrincipalWithRoles("admin")));
    }

    [Fact]
    public void Authorize_Anonymous_ReturnsOkWithoutLogin()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = new TestFunction();
        function.AccessType = FunctionAccessType.Anonymous;

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Equal(AuthorizationStatus.Ok, authorization.Authorize(function, anonymous));
    }

    [Fact]
    public void Authorize_Unauthenticated_RoleLimit_ReturnsUnauthorized()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("admin");

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Equal(AuthorizationStatus.Unauthorized, authorization.Authorize(function, anonymous));
    }

    [Fact]
    public void GetOkRoles_ReturnsMatchedRolesOnly()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = RoleLimitFunction("a,b");

        var okRoles = authorization.GetOkRoles(function, PrincipalWithRoles("b", "c")).ToArray();

        Assert.Single(okRoles);
        Assert.Equal("b", okRoles[0]);
    }

    [Fact]
    public void GetOkRoles_NotRoleLimit_ReturnsAllUserRoles()
    {
        var authorization = new TestAuthorization(BuildConfiguration());
        var function = new TestFunction();
        function.AccessType = FunctionAccessType.Login;

        var okRoles = authorization.GetOkRoles(function, PrincipalWithRoles("x", "y")).ToArray();

        Assert.Equal(new[] { "x", "y" }, okRoles);
    }

    [Fact]
    public void GetRoles_SameIdentity_ReturnsStableResult()
    {
        // ClaimsIdentity 角色解析已加 ConditionalWeakTable 缓存，多次调用结果必须一致
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(System.Security.Claims.ClaimTypes.Role, "admin"),
            new Claim(System.Security.Claims.ClaimTypes.Role, "ops,dev")
        }, "test");

        var first = identity.GetRoles().ToArray();
        var second = identity.GetRoles().ToArray();

        Assert.Equal(new[] { "admin", "ops", "dev" }, first);
        Assert.Equal(first, second);
    }
}
