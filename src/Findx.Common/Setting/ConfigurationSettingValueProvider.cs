namespace Findx.Setting
{
    /// <summary>
    /// 默认配置提供器
    /// </summary>
    public class ConfigurationSettingValueProvider : ISettingValueProvider
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigurationSettingValueProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取key对象
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>(string key) where T : new()
        {
            return _configuration.GetSection(key).Get<T>();
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
        
        /// <summary>
        /// 提供器名称
        /// </summary>
        public string Name => "Configuration";
        
        /// <summary>
        /// 排序号
        /// </summary>
        public int Order => 0;
    }
}

