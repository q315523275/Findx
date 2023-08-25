using System.Collections.Generic;
using FreeSql;
using FreeSql.Internal;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql
{
    /// <summary>
    ///     FreeSql参数
    /// </summary>
    public class FreeSqlOptions : IOptions<FreeSqlOptions>
    {
        /// <summary>
        ///     是否启用模块
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     默认数据源
        /// </summary>
        public string Primary { set; get; }

        /// <summary>
        ///     严格匹配数据源,默认false. true未匹配到指定数据源时抛异常,false使用默认数据源
        /// </summary>
        public bool Strict { set; get; }

        /// <summary>
        ///     检查插入属性
        /// </summary>
        public bool CheckInsert { set; get; }

        /// <summary>
        ///     检查更新属性
        /// </summary>
        public bool CheckUpdate { set; get; }

        /// <summary>
        ///     数据源列表
        /// </summary>
        public Dictionary<string, FreeSqlConnectionConfig> DataSource { set; get; } = new();

        /// <summary>
        ///     this
        /// </summary>
        public FreeSqlOptions Value => this;
    }

    /// <summary>
    ///     FreeSql数据库连接配置
    /// </summary>
    public class FreeSqlConnectionConfig
    {
        /// <summary>
        ///     数据库连接字符串
        /// </summary>
        public string ConnectionString { set; get; }

        /// <summary>
        ///     数据库类型
        /// </summary>
        public DataType DbType { set; get; }

        /// <summary>
        ///     数据源共享
        /// </summary>
        public List<string> DataSourceSharing { set; get; } = new List<string>();

        /// <summary>
        ///     字段名称转换类型
        /// </summary>
        public NameConvertType NameConvertType { set; get; } = NameConvertType.None;

        /// <summary>
        ///     是否打印SQL日志调试
        /// </summary>
        public bool PrintSql { set; get; }

        /// <summary>
        ///     是否开启慢SQL记录
        /// </summary>
        public bool OutageDetection { set; get; }

        /// <summary>
        ///     慢SQL记录标准2秒
        /// </summary>
        public int OutageDetectionInterval { set; get; } = 2;

        /// <summary>
        ///     是否逻辑删除
        /// </summary>
        public bool SoftDeletable { set; get; }

        /// <summary>
        ///     是否多租户
        /// </summary>
        public bool MultiTenant { set; get; }

        /// <summary>
        ///     多租户字段名
        /// </summary>
        public string MultiTenantFieldName { set; get; } = "TenantId";

        /// <summary>
        ///     自动同步实体结构【开发环境必备】，FreeSql不会扫描程序集，只有CRUD时才会生成表。
        /// </summary>
        public bool UseAutoSyncStructure { set; get; }
    }
}