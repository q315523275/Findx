using System.Collections.Generic;

namespace Findx.Configuration
{
    /// <summary>
    /// 配置Dto
    /// </summary>
    internal class ConfigDto
    {
        /// <summary>
        /// 当前配置版本号
        /// </summary>
        public long Version { set; get; }
        
        /// <summary>
        /// 配置集合
        /// </summary>
        public List<KeyValueDto> Data { set; get; }
    }
    /// <summary>
    /// KeyValueDto
    /// </summary>
    internal class KeyValueDto
    {
        /// <summary>
        /// 配置项类型
        /// </summary>
        public VaultType VaultType { set; get; }
        
        /// <summary>
        /// 配置项Key
        /// </summary>
        public string VaultKey { set; get; }
        
        /// <summary>
        /// 配置项值
        /// </summary>
        public string Vault { set; get; }
    }
    internal enum VaultType
    {
        Text = 0,
        Json = 1
    }
}
