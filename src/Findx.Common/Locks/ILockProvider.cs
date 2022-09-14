namespace Findx.Locks
{
	/// <summary>
	/// 锁提供器
	/// </summary>
	public interface ILockProvider
	{
		/// <summary>
        /// 获取指定锁服务
        /// </summary>
        /// <param name="lockType"></param>
        /// <returns></returns>
		ILock Get(LockType lockType);
	}
}

