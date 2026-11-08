using System;
using Findx.Data;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     实体扩展属性缓存回归测试
///     <para>覆盖 GetEntityExtensionAttribute 改造后的语义：TryGetValue 命中路径返回同一实例、
///     反射解析只发生一次、软删除/分表标记由基类接口推断、不同实体类型互不干扰。</para>
/// </summary>
public class EntityExtensionsTests
{
    private sealed class PlainEntity : EntityBase<long>
    {
    }

    [EntityExtension(DataSource = "slave", DisableAuditing = true)]
    private sealed class AnnotatedEntity : EntityBase<long>
    {
    }

    private sealed class SoftDeleteEntity : ISoftDeletable
    {
        public DateTime? DeletionTime { get; set; }

        public bool IsDeleted { get; set; }
    }

    [Fact]
    public void GetEntityExtensionAttribute_RepeatedCalls_ReturnSameInstance()
    {
        // 第二次调用走 TryGetValue 快路径，必须命中同一缓存实例（不再新建 Attribute）
        var first = typeof(PlainEntity).GetEntityExtensionAttribute();
        var second = typeof(PlainEntity).GetEntityExtensionAttribute();

        Assert.Same(first, second);
    }

    [Fact]
    public void GetEntityExtensionAttribute_AttributeValuesApplied()
    {
        var attribute = typeof(AnnotatedEntity).GetEntityExtensionAttribute();

        Assert.Equal("slave", attribute.DataSource);
        Assert.True(attribute.DisableAuditing);
    }

    [Fact]
    public void GetEntityExtensionAttribute_SoftDeleteInterface_SetsHasSoftDeletable()
    {
        var attribute = typeof(SoftDeleteEntity).GetEntityExtensionAttribute();

        Assert.True(attribute.HasSoftDeletable);
    }

    [Fact]
    public void GetEntityExtensionAttribute_PlainEntity_HasSoftDeletableFalse()
    {
        var attribute = typeof(PlainEntity).GetEntityExtensionAttribute();

        Assert.False(attribute.HasSoftDeletable);
    }

    [Fact]
    public void GetEntityExtensionAttribute_DifferentTypes_CachedSeparately()
    {
        var plain = typeof(PlainEntity).GetEntityExtensionAttribute();
        var annotated = typeof(AnnotatedEntity).GetEntityExtensionAttribute();

        Assert.NotSame(plain, annotated);
        Assert.Equal("slave", annotated.DataSource);
        Assert.Null(plain.DataSource);
    }

    [Fact]
    public void GetEntityExtensionAttribute_FromEntityInstance_SameAsTypeCall()
    {
        var entity = new PlainEntity();

        var fromInstance = entity.GetEntityExtensionAttribute();
        var fromType = typeof(PlainEntity).GetEntityExtensionAttribute();

        Assert.Same(fromInstance, fromType);
    }
}
