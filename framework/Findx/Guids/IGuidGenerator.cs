namespace Findx.Guids;

/// <summary>
///     Guid生成器
///		<para>默认为Abp非连续递增Guid生成,,如需连续递增请使用Findx.Guid.NewId组件包</para>
/// </summary>
public interface IGuidGenerator
{
	/// <summary>
	///     生成
	/// </summary>
	/// <returns></returns>
	Guid Create();
}