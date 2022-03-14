using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    public class SysConfigRequest : IRequest
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
        /// 常量所属分类的编码，来自于“常量的分类”字典
        /// </summary>
        public string GroupCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否是系统参数（Y-是，N-否）
        /// </summary>
        public string SysFlag { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}
