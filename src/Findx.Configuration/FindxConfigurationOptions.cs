namespace Findx.Configuration
{
    /// <summary>
    /// Findx配置中心控制配置
    /// </summary>
    public class FindxConfigurationOptions
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string AppId { set; get; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string AppSercet { set; get; }
        /// <summary>
        /// 组
        /// </summary>
        public string Group { set; get; }
        /// <summary>
        /// 名称空间
        /// </summary>
        public string Namespace { set; get; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Endpoint { set; get; }
        /// <summary>
        /// 轮询间隔时间
        /// 单位秒
        /// </summary>
        public int RefreshInteval { set; get; } = 60;
    }
}
