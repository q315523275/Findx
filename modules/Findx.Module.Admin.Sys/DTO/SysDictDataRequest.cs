using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 系统字典值请求如惨
    /// </summary>
    public class SysDictDataRequest : IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 字典类型id
        /// </summary>
        public long TypeId { get; set; }


        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}
