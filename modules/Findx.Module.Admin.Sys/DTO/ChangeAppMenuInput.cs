using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 切换应用菜单
    /// </summary>
    public class ChangeAppMenuInput
    {
        /// <summary>
        /// 应用编码
        /// </summary>DeleteMenuInput
        [Required(ErrorMessage = "应用编码不能为空")]
        public string Application { get; set; }
    }
}
