using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    public class SysOrgRequest : IRequest
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
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父id
        /// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 父ids
        /// </summary>
        public string Pids { get; set; }

        /// <summary>
        /// 描述
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
    }
}
