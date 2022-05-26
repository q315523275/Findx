using System;
namespace Findx.Mapping
{
	/// <summary>
    /// 忽略映射
    /// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class IgnoreMappingAttribute: Attribute
	{
	}
}

