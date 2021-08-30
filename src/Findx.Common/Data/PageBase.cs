using System.ComponentModel.DataAnnotations;
namespace Findx.Data
{
    /// <summary>
    /// 查询请求分页基类
    /// </summary>
    public class PageBase : IPager
    {
        /// <summary>
        /// 当前分页数
        /// 默认：1
        /// </summary>
        [Range(1, 9999)]
        public virtual int PageNo { get; set; } = 1;
        /// <summary>
        /// 当前分页记录数
        /// 默认：2
        /// </summary>
        [Range(1, 9999)]
        public virtual int PageSize { get; set; } = 20;
        /// <summary>
        /// 分页条件
        /// </summary>
        public virtual string Order { get; set; } = "id";
        /// <summary>
        /// 排序
        /// </summary>
        public virtual bool Asc { get; set; } = true;
    }
}
