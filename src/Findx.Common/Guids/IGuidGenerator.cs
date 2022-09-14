using System;
namespace Findx.Guids
{
	/// <summary>
	/// Guid生成器
	/// </summary>
	public interface IGuidGenerator
	{
		/// <summary>
		/// 生成
		/// </summary>
		/// <returns></returns>
		Guid Create();
	}
}

