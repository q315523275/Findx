using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Findx.Data
{
    /// <summary>
    /// 仓储使用多排
    /// </summary>
    public class MultiOrderBy<TEntity>
    {
        public List<OrderByParameter<TEntity>> OrderBy { set; get; } = new List<OrderByParameter<TEntity>>();
    }
    /// <summary>
    /// 排序参数
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class OrderByParameter<TEntity>
    {
        /// <summary>
        /// 排序字段控制
        /// </summary>
        public Expression<Func<TEntity, object>> Expression { set; get; }
        /// <summary>
        /// 正序
        /// </summary>
        public bool Ascending = true;
    }
}
