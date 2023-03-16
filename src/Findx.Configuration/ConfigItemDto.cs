﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Findx.Configuration
{
    /// <summary>
    /// 配置项Dto
    /// </summary>
    public class ConfigItemDto
    {
        /// <summary>
        /// 数据编号
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataType DataType { get; set; }
    
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public long Version { get; set; }
    }
    public enum DataType
    {
        Text = 0,
        Json = 1
    }
}