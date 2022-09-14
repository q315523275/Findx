namespace Findx.Setting
{
    /// <summary>
    /// 默认配置提供器
    /// </summary>
    public class ConfigurationSettingProvider : ISettingProvider
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigurationSettingProvider(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// 获取key对象
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>(string key) where T : new()
        {
            return _configuration.GetSection("key").Get<T>();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }
}

