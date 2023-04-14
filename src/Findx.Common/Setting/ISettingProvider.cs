namespace Findx.Setting
{
	/// <summary>
    /// 配置提供器
    /// </summary>
	public interface ISettingProvider
	{
		/// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
		T GetValue<T>(string key);

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetObject<T>(string key) where T : new();
	}
}

