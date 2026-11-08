using Findx.Data;

namespace Findx.UnitTests;

/// <summary>
///     AuditHashUtility 哈希链工具回归测试（GxP 完整性：SHA256 链计算稳定、明细篡改敏感、链衔接参与）
/// </summary>
public class AuditHashUtilityTests
{
    private static readonly string[] Head =
    [
        "tenant-1", "user-1", "admin", "管理员", "用户管理.查询",
        "127.0.0.1", "Mozilla", "/api/users", "GET",
        "200", "2026-11-03T10:00:00.0000000+08:00", "12",
        null, "{\"page\":1}", "{\"total\":1}"
    ];

    private static readonly string[] Entities =
    [
        "Findx.Module.EleAdminPlus.Shared.Models.SysUserInfo|1|Username|admin|admin2|Success",
        "Findx.Module.EleAdminPlus.Shared.Models.SysUserInfo|1|Phone|13800000000|13900000000|Success"
    ];

    private static readonly string[] Sqls =
    [
        "sys_users|UPDATE sys_users SET Username=@p0 WHERE Id=1|Success"
    ];

    [Fact]
    public void Compute_Stable_Deterministic()
    {
        var a = AuditHashUtility.Compute(Head, Entities, Sqls, null);
        var b = AuditHashUtility.Compute(Head, Entities, Sqls, null);

        Assert.Equal(a, b);
        Assert.Equal(64, a.Length); // SHA256 hex
    }

    [Fact]
    public void Compute_PrevHashParticipates_InChain()
    {
        var withPrev = AuditHashUtility.Compute(Head, Entities, Sqls, "ABC");
        var withoutPrev = AuditHashUtility.Compute(Head, Entities, Sqls, null);

        Assert.NotEqual(withPrev, withoutPrev);
        // 同 prev 链结果仍稳定
        Assert.Equal(withPrev, AuditHashUtility.Compute(Head, Entities, Sqls, "ABC"));
    }

    [Fact]
    public void Compute_EntityRowOrder_Irrelevant()
    {
        var ordered = AuditHashUtility.Compute(Head, Entities, Sqls, "prev");
        var reversed = AuditHashUtility.Compute(Head, Entities.Reverse(), Sqls, "prev");

        Assert.Equal(ordered, reversed);
    }

    [Fact]
    public void Compute_TamperedDetail_ChangesHash()
    {
        var original = AuditHashUtility.Compute(Head, Entities, Sqls, "prev");
        var tampered = AuditHashUtility.Compute(Head, ["...|1|Username|admin|HACKED|Success"], Sqls, "prev");

        Assert.NotEqual(original, tampered);
    }

    [Fact]
    public void Compute_NullInputs_NoThrow()
    {
        var hash = AuditHashUtility.Compute(null, null, null, null);

        Assert.Equal(64, hash.Length);
        // null 头与空头等价（Normalize 后均空串）
        Assert.Equal(hash, AuditHashUtility.Compute([], [], [], null));
    }

    [Fact]
    public void ComputeSummary_IgnoresEmptyRows_StableOrder()
    {
        var a = AuditHashUtility.ComputeSummary(["", "b", null, "a"]);
        var b = AuditHashUtility.ComputeSummary(["a", "b"]);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Normalize_NullToEmpty()
    {
        Assert.Equal("", AuditHashUtility.Normalize(null));
        Assert.Equal("x", AuditHashUtility.Normalize("x"));
    }
}
