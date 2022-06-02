using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models
{
	/// <summary>
	/// 菜单
	/// </summary>
	[Table(Name = "sys_menu")]
	public class SysMenuInfo : EntityBaseFullAudited<Guid, Guid>, ISoftDeletable, ITenant, ISort, IResponse
	{
		/// <summary>
		/// 菜单id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override Guid Id { get; set; }

		/// <summary>
		/// 上级id, 0是顶级
		/// </summary>
		public int ParentId { get; set; }

		/// <summary>
		/// 菜单名称
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 菜单路由地址
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// 菜单组件地址, 目录可为空
		/// </summary>
		public string Component { get; set; }

		/// <summary>
		/// 类型, 0菜单, 1按钮
		/// </summary>
		public int MenuType { get; set; }

		/// <summary>
		/// 排序号
		/// </summary>
		public int Sort { get; set; }

		/// <summary>
		/// 权限标识
		/// </summary>
		public string Authority { get; set; }

		/// <summary>
		/// 打开位置
		/// </summary>
		public string Target { get; set; }

		/// <summary>
		/// 菜单图标
		/// </summary>
		public string Icon { get; set; }

		/// <summary>
		/// 图标颜色
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// 是否隐藏, 0否, 1是(仅注册路由不显示在左侧菜单)
		/// </summary>
		public int Hide { get; set; }

		/// <summary>
		/// 菜单侧栏选中的path
		/// </summary>
		public string Active { get; set; }

		/// <summary>
		/// 其它路由元信息
		/// </summary>
		public string Meta { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public Guid? TenantId { get; set; }
		
		/// <summary>
		/// 是否删除
		/// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// 删除时间
		/// </summary>
		public DateTime? DeletionTime { get; set; }
	}
}

