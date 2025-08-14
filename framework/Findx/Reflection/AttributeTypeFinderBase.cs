namespace Findx.Reflection;

/// <summary>
///     标注了指定<see cref="Attribute" />特性的类型查找器基类
/// </summary>
/// <typeparam name="TAttributeType">要查找的特性注解</typeparam>
public abstract class AttributeTypeFinderBase<TAttributeType> : FinderBase<Type>, ITypeFinder where TAttributeType : Attribute
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        return AssemblyManager.FindTypesByAttribute<TAttributeType>();
    }
}