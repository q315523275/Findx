using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Findx.Data
{
    /// <summary>
    /// 排序参数
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class OrderByParameter<TEntity>
    {
        /// <summary>
        /// 排序字段表达式
        /// </summary>
        public Expression<Func<TEntity, object>> Expression { set; get; }

        /// <summary>
        /// 排序方向
        /// </summary>
        public ListSortDirection SortDirection { set; get; } = ListSortDirection.Descending;
    }
}
