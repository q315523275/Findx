using System.ComponentModel;

namespace Findx.Locks
{
    public enum LockType
	{
		/// <summary>
		/// 本地
		/// </summary>
		[Description("本地")]
		Local,

		/// <summary>
		/// 分布式锁
		/// </summary>
		[Description("分布式锁")]
		Distributed,
	}
}

